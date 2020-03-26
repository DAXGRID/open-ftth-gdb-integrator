using OpenFTTH.GDBIntegrator.Subscriber;

namespace OpenFTTH.GDBIntegrator
{
    public class Startup
    {
        private ISubscriber _subscriber;

        public Startup(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public void Start()
        {
            _subscriber.Subscribe();

            System.Console.ReadLine();
            _subscriber.Dispose();
        }
    }
}
