using Extrator.MessageContext;
using Extrator.Service;
using Extrator.SQLContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extrator.Job
{
    public class RecurringJob : IHostedService, IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private readonly IConfigurationRoot config;
        private readonly IDatabase database;
        private readonly IMessage message;

        public RecurringJob(IConfigurationRoot config, IDatabase database, IMessage message)
        {
            this.config = config;
            this.database = database;
            this.message = message;
        }

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // This will cause the loop to stop if the service is stopped
            while (!stoppingToken.IsCancellationRequested)
            {
                Logger.Info("Checking for changes...");
                var service = new ListenTablesService(new Factory.Factory(config), config);
                var sectionChanges = service.CheckSectionChanges();
                foreach (var section in sectionChanges)
                {
                    Logger.Info($"Sending message for section {section}");
                    var data = service.GetMessageData(section);
                    foreach (var item in data)
                    {
                        message.SendMessage(item);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Store the task we're executing
            _executingTask = ExecuteAsync(_stoppingCts.Token);

            // If the task is completed then return it,
            // this will bubble cancellation and failure to the caller
            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            // Otherwise it's running
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite,
                    cancellationToken));
            }
        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }
}
