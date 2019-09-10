using Extrator.MessageContext;
using Extrator.MessageContext.ServiceBUS;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extrator.Factory
{
    public class MessagingServiceFactory
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IConfigurationRoot config;

        public MessagingServiceFactory(IConfigurationRoot config)
        {
            this.config = config;
        }

        public IMessage GetMessageService()
        {
            Logger.Debug("Getting messaging config...");
            var messagingService = config.GetSection("QueueService").Value;
            if (string.IsNullOrEmpty(messagingService)) throw new NullReferenceException("[QueueService]");
            switch (messagingService.ToUpperInvariant())
            {
                case "SERVICEBUS":
                    return new ServiceBUS(config);
                default:
                    {
                        Logger.Fatal($"Messaging Service not implemented: {messagingService}", new NotImplementedException($"[QueueService]: {messagingService}"));
                        throw new NotImplementedException($"[QueueService]: {messagingService}");
                    }
            }
        }
    }
}
