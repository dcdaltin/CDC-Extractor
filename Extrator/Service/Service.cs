namespace Extrator.Service
{
    using Extrator.Factory;
    using Microsoft.Extensions.Configuration;
    using System;

    public class Service : IService
    {
        private readonly IConfiguration config;
        private readonly IFactory factory;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Service(IConfiguration config, IFactory factory)
        {
            this.config = config;
            this.factory = factory;
        }

        public IListenTable GetListenService()
        {
            var module = config.GetSection("Module").Value;
            if (string.IsNullOrEmpty(module)) throw new NullReferenceException("[Module]");
            switch (module.ToUpperInvariant())
            {
                case "CDC": return new CDCService(factory, config);
                case "ALL": return new ListenTablesService(factory, config);
                default:
                    {
                        Logger.Fatal($"Module not implemented: {module}", new NotImplementedException($"[Module]: {module}"));
                        throw new NotImplementedException($"[Module]: {module}");
                    }
            }
        }
    }
}
