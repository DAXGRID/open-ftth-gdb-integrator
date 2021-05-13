namespace OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages
{
    public class InvalidMessage
    {
        public object Message { get; }
        public bool Delete { get; }

        public InvalidMessage(object message, bool delete = false)
        {
            Message = message;
            Delete = delete;
        }
    }
}
