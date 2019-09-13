namespace Extrator.Job
{
    using Extrator.Service;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IService _service;
        private readonly IConfiguration _config;
        private Timer _timer;

        public TimedHostedService(ILogger<TimedHostedService> logger, IService service, IConfiguration config)
        {
            _logger = logger;
            _service = service;
            _config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");
            var interval = _config.GetSection("ListenInterval").Value;
            if (string.IsNullOrEmpty(interval)) throw new NullReferenceException("[ListenInterval]");
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(int.Parse(interval)));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _service.GetListenService().Run();
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
