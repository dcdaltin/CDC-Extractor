namespace Extrator
{
    using Extrator.Factory;
    using Extrator.Job;
    using System.Threading.Tasks;

    class Program
    {
        public static void Main(string[] args)
        {
            var configFactory = new ConfigFactory();
            configFactory.BuildLogConfig();
            var config = configFactory.BuildConfig();
            var database = new DatabaseFactory(config).GetDatabase();
            var message = new MessagingServiceFactory(config).GetMessageService();
            var job = new RecurringJob(config, database, message);
            Parallel.Invoke(async () => await job.StartAsync(new System.Threading.CancellationToken()));
        }
    }
}
