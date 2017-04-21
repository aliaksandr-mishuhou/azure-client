using System;
using System.Reflection;
using System.Threading;
using EventGateway.Core;
using Ninject;

namespace EventGateway.Host
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var exitEvent = new ManualResetEvent(false);

            var kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            var service = kernel.Get<IMessageListener>();
            service.Start();


            Console.CancelKeyPress += (sender, eventArgs) => {
                service.Stop();
                exitEvent.Set();
            };

            exitEvent.WaitOne();
        }
    }
}
