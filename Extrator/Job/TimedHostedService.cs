namespace Extrator.Job
{
    using Extrator.Factory;
    using Extrator.Service;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IFactory _factory;
        private readonly IListenTableService _listenService;
        private Timer _timer;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public TimedHostedService(ILogger<TimedHostedService> logger, IFactory factory, IListenTableService listenService)
        {
            _logger = logger;
            _factory = factory;
            _listenService = listenService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Timed Background Service is working.");
            Logger.Info("Checking for changes...");
            var sectionChanges = _listenService.CheckSectionChanges();
            foreach (var section in sectionChanges)
            {
                Logger.Info($"Sending message for section {section}");
                var data = _listenService.GetMessageData(section);
                foreach (var item in data)
                {
                    _factory.GetMessagingService().SendMessage(item);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
