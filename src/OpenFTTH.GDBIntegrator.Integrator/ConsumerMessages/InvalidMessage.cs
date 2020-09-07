namespace OpenFTTH.GDBIntegrator.Integrator.ConsumerMessages
{
    public class InvalidMessage
    {
        public object Message { get; }

        public InvalidMessage(object message)
        {
            Message = message;
        }
    }
}
