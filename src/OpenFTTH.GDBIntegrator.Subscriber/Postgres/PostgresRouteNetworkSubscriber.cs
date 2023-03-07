using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenFTTH.GDBIntegrator.Config;
using OpenFTTH.GDBIntegrator.Integrator.Commands;
using OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages;
using OpenFTTH.GDBIntegrator.RouteNetwork.Mapping;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OpenFTTH.GDBIntegrator.Subscriber.Postgres;

public sealed class PostgresRouteNetworkSubscriber : IRouteNetworkSubscriber
{
    const long DEFAULT_LATEST_SEQ_NO_ID = 1;
    private readonly EventStoreSetting _eventStoreSetting;
    private readonly PostgisSetting _postgisSetting;
    private readonly IInfoMapper _infoMapper;
    private readonly IMediator _mediator;
    private readonly ILogger<PostgresRouteNetworkSubscriber> _logger;
    private Channel<RouteNetworkEditOperation> _editOperationCh;
    private bool _started = false;

    public PostgresRouteNetworkSubscriber(
        IOptions<EventStoreSetting> eventStoreSetting,
        IOptions<PostgisSetting> postgisSetting,
        IInfoMapper infoMapper,
        IMediator mediator,
        ILogger<PostgresRouteNetworkSubscriber> logger)
    {
        _eventStoreSetting = eventStoreSetting.Value;
        _postgisSetting = postgisSetting.Value;
        _infoMapper = infoMapper;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Subscribe(
        int intervalMs,
        CancellationToken token = default)
    {
        if (_started)
        {
            throw new InvalidOperationException("The subscription has already been started.");
        }

        // Do this to avoid it being started multiple times, this can
        // cause the channel to not be disposed.
        _started = true;

        // Create table if not exists
        await SetupLatestSequenceNumberSchemaAndTable().ConfigureAwait(false);

        var latestProcessedEventNumber = await GetLatestSequenceNumber(DEFAULT_LATEST_SEQ_NO_ID);

        _editOperationCh = Channel.CreateUnbounded<RouteNetworkEditOperation>();

        var subscribeEditOperationEventsTask = Task.Run(async () =>
        {
            try
            {
                await SubscribeEditOperationEvents(latestProcessedEventNumber, intervalMs, token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _editOperationCh.Writer.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError("{Exception}", ex);
                _editOperationCh.Writer.Complete();
                throw;
            }
        }, token);

        var HandleEditOperationsTask = Task.Run(async () =>
        {
            try
            {
                await HandleEditOperations(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _editOperationCh.Writer.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError("{Exception}", ex);
                _editOperationCh.Writer.Complete();
                throw;
            }
        }, token);

        await Task
            .WhenAll(subscribeEditOperationEventsTask, HandleEditOperationsTask)
            .ConfigureAwait(false);
    }

    private async Task HandleEditOperations(CancellationToken token)
    {
        var nodeFactory = new RouteNodeMessageFactory(_infoMapper);
        var segmentFactory = new RouteSegmentMessageFactory(_infoMapper);

        await foreach (var editOperation in _editOperationCh.Reader.ReadAllAsync(token).ConfigureAwait(false))
        {
            if (editOperation.Type == RouteNetworkEditTypeName.RouteNode)
            {
                var routeNodeMessage = nodeFactory.Create(editOperation);
                if (routeNodeMessage.After is null)
                {
                    // We do nothing, the element has been deleted has been deleted
                }
                else if (routeNodeMessage.After.Coord is null)
                {
                    _logger.LogWarning(
                        $"Received invalid message where after.coord is null: {routeNodeMessage}",
                        JsonSerializer.Serialize(routeNodeMessage));

                    await _mediator.Send(new GeoDatabaseUpdated
                    {
                        UpdateMessage = new InvalidMessage(routeNodeMessage, editOperation.EventId)
                    });
                }
                else
                {
                    await _mediator.Send(new GeoDatabaseUpdated { UpdateMessage = routeNodeMessage });
                }
            }
            else if (editOperation.Type == RouteNetworkEditTypeName.RouteSegment)
            {
                var routeSegmentMessage = segmentFactory.Create(editOperation);
                if (routeSegmentMessage.After is null)
                {
                    // We do nothing, the element has been deleted has been deleted
                }
                else if (routeSegmentMessage.After.Coord is null)
                {
                    InvalidMessage invalidMessage = null;

                    // Cannot roll back this should never happen, but we have seen it once.
                    if (routeSegmentMessage?.Before != null && routeSegmentMessage.Before?.Coord is null)
                    {
                        // In case this happens we delete the node.
                        invalidMessage = new InvalidMessage(routeSegmentMessage, editOperation.EventId, true);
                    }
                    else
                    {
                        invalidMessage = new InvalidMessage(routeSegmentMessage, editOperation.EventId);
                    }

                    _logger.LogWarning(
                        "Received invalid message where after.coord is null: {RouteSegmentMessage}",
                        JsonSerializer.Serialize(routeSegmentMessage));

                    await _mediator.Send(new GeoDatabaseUpdated { UpdateMessage = invalidMessage });
                }
                else
                {
                    await _mediator.Send(new GeoDatabaseUpdated { UpdateMessage = routeSegmentMessage });
                }
            }
            else
            {
                throw new ArgumentException($"Cannot handle type of '{editOperation.Type}'.");
            }

            await InsertSequenceNumber(DEFAULT_LATEST_SEQ_NO_ID, editOperation.SequenceNumber)
                .ConfigureAwait(false);
        }
    }

    private async Task SubscribeEditOperationEvents(
        long latestProcessedSequenceNumber,
        int intervalMs,
        CancellationToken token = default)
    {
        const string GET_LATEST_SQL = @"SELECT *
FROM route_network.route_network_edit_operation
WHERE seq_no > @seq_no
ORDER BY seq_no";

        while (!token.IsCancellationRequested)
        {
            using var conn = new NpgsqlConnection(_postgisSetting.ConnectionString);
            using var cmd = new NpgsqlCommand(GET_LATEST_SQL, conn);
            cmd.Parameters.AddWithValue("seq_no", latestProcessedSequenceNumber);

            await conn.OpenAsync(token).ConfigureAwait(false);

            var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);

            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                var sequenceNumber = reader.GetInt64(reader.GetOrdinal("seq_no"));
                var eventId = reader.GetGuid(reader.GetOrdinal("event_id"));
                var before = reader.GetValue(reader.GetOrdinal("before")) as string ?? null;
                var after = reader.GetValue(reader.GetOrdinal("after")) as string ?? null;
                var type = reader.GetString(reader.GetOrdinal("type"));

                var editOperation = new RouteNetworkEditOperation(
                    sequenceNumber: sequenceNumber,
                    eventId: eventId,
                    before: before,
                    after: after,
                    type: type
                );

                await _editOperationCh.Writer.WriteAsync(editOperation).ConfigureAwait(false);

                latestProcessedSequenceNumber = sequenceNumber;
            }

            await Task.Delay(intervalMs, token).ConfigureAwait(false);
        }
    }

    public async Task SetupLatestSequenceNumberSchemaAndTable()
    {
        const string SQL = @"
CREATE SCHEMA IF NOT EXISTS gdb_integrator;
CREATE TABLE IF NOT EXISTS gdb_integrator.latest_edit_operation (
    id SERIAL PRIMARY KEY,
    latest_seq_no BIGINT);";

        using var conn = new NpgsqlConnection(_eventStoreSetting.ConnectionString);
        using var cmd = new NpgsqlCommand(SQL, conn);

        await conn.OpenAsync().ConfigureAwait(false);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task<long> GetLatestSequenceNumber(long id)
    {
        // We just return 0 if none is found, this happens the first time the application starts up and no
        // latest edit operation sequence number has been stored.
        const string SQL = "SELECT COALESCE((SELECT latest_seq_no FROM gdb_integrator.latest_edit_operation WHERE id = @id), 0);";

        using var conn = new NpgsqlConnection(_eventStoreSetting.ConnectionString);
        using var cmd = new NpgsqlCommand(SQL, conn);
        cmd.Parameters.AddWithValue("id", id);

        await conn.OpenAsync().ConfigureAwait(false);
        var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        return (long)result;
    }

    public async Task InsertSequenceNumber(long id, long sequenceNumber)
    {
        const string SQL = @"
INSERT INTO gdb_integrator.latest_edit_operation (id, latest_seq_no)
VALUES (@id, @seq_no)
ON CONFLICT (id)
DO UPDATE SET latest_seq_no = EXCLUDED.latest_seq_no;";

        using var conn = new NpgsqlConnection(_eventStoreSetting.ConnectionString);
        using var cmd = new NpgsqlCommand(SQL, conn);

        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("seq_no", sequenceNumber);

        await conn.OpenAsync().ConfigureAwait(false);
        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_editOperationCh is not null)
        {
            _editOperationCh.Writer.Complete();
        }
    }
}
