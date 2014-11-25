using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Mono.Options;

namespace Miss
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            int port = 11550;
            // TODO(bl-nero): Investigate different connection options, especially on Windows, and
            // refactor this crude setting into something easier to use.
            string brickConnection = "/dev/tty.EV3-SerialPort";

            var optionSet = new OptionSet()
            {
                { "p|port:", (int p) => port = p },
                { "c|connection:", c => brickConnection = c }
            };
            optionSet.Parse(args);

            INancyBootstrapper bootstrapper = "dummy".Equals(brickConnection) ?
                (INancyBootstrapper)new DummyMissBootstrapper() :
                new DefaultMissBootstrapper(brickConnection);

            using (var nancyHost = new NancyHost(bootstrapper, new Uri("http://localhost:" + port)))
            {
                var exitEvent = new ManualResetEvent(false);

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    exitEvent.Set();
                };

                nancyHost.Start();
                exitEvent.WaitOne();
            }
        }
    }
}
