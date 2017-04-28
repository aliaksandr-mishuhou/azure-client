using System;
using System.Configuration;
using Contour;
using Contour.Configurator;
using Contour.Receiving.Consumers;
using EventGateway.Azure;
using EventGateway.Core;
using EventGateway.CP;
using EventGateway.RestClient;
using Newtonsoft.Json.Linq;
using Ninject;
using Ninject.Modules;

namespace EventGateway.Host.DI
{
    public class Module : NinjectModule
    {
        private const string BusEndpointName = "EventGateway";

        public override void Load()
        {
            LoadIncomingMessageServices();
            LoadProcessorServices();
        }

        private void LoadProcessorServices()
        {
            //// direct Azure message publisher
            //var connectionString = ConfigurationManager.AppSettings["Azure.EventHubs.ConnectionString"];
            //var entityPath = ConfigurationManager.AppSettings["Azure.EventHubs.EntityPath"];
            //this.Bind<IMessageProcessor>()
            //    .ToConstructor(ctx => new EventHubMessageProcessor(connectionString, entityPath))
            //    .InSingletonScope()
            //    .Named("AzureHubMessageProcessor");

            // API proxy message publisher
            var baseUrl = ConfigurationManager.AppSettings["RestClient.BaseUrl"];
            this.Bind<IMessageProcessor>()
                .ToConstructor(ctx => new RestApiMessageProcessor(baseUrl))
                .InSingletonScope()
                .Named("RestApiMessageProcessor");
        }

        private void LoadIncomingMessageServices()
        {
            this.Bind<IBus>()
                .ToMethod(
                    ctx => new BusFactory().Create(
                        cfg =>
                        {
                            ctx.Kernel.Get<IConfigurator>()
                                .Configure(BusEndpointName, cfg);
                        }))
                .InSingletonScope();

            this.Bind<IConfigurator>()
                .ToConstructor(ctx => new AppConfigConfigurator(new Sbdr(this.Kernel)))
                .InSingletonScope();

            this.Bind<IConsumerOf<JObject>>()
                .To<MessageConsumer>()
                .InSingletonScope();

            this.Bind<IMessageListener>()
                .To<MessageListener>()
                .InSingletonScope();
        }

        private class Sbdr : IDependencyResolver
        {
            private readonly IKernel kernel;

            public Sbdr(IKernel kernel)
            {
                this.kernel = kernel;
            }

            public object Resolve(string name, Type type)
            {
                return this.kernel.Get(type);
            }
        }
    }
}
