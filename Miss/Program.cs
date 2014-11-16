using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MonoBrick.EV3;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;
using Mono.Options;

namespace Miss
{
    class MissBootstrapper: DefaultNancyBootstrapper
    {
        private Brick<Sensor, Sensor, Sensor, Sensor> brick;

        public MissBootstrapper(Brick<Sensor, Sensor, Sensor, Sensor> brick)
        {
            this.brick = brick;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(
                typeof(IDictionary<char, MotorWrapper>),
                new Dictionary<char, MotorWrapper>()
                {
                    { 'a', new MotorWrapper(brick.MotorA) },
                    { 'b', new MotorWrapper(brick.MotorB) },
                    { 'c', new MotorWrapper(brick.MotorC) },
                    { 'd', new MotorWrapper(brick.MotorD) },
                });
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.BeforeRequest += ctx =>
            {
                if (ctx.Request.Headers["Origin"].FirstOrDefault() != "http://snap.berkeley.edu")
                {
                    return new Response().WithStatusCode(400);
                }
                return null;
            };

            pipelines.AfterRequest += ctx =>
            {
                ctx.Response.WithHeader("Access-Control-Allow-Origin", "http://snap.berkeley.edu");
            };

            pipelines.OnError += (ctx, ex) =>
            {
                Console.WriteLine(ex);
                return null;
            };
        }
    }

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

            var brick = new Brick<Sensor, Sensor, Sensor, Sensor>(brickConnection);
            Console.WriteLine(String.Format("Connecting to the brick at {0}", brickConnection));
            brick.Connection.Open();

            try
            {
                using (var nancyHost = new NancyHost(
                                       new MissBootstrapper(brick),
                                       new Uri("http://localhost:" + port)))
                {
                    nancyHost.Start();
                    Console.WriteLine(String.Format("Server started on port {0}.", port));
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            finally
            {
                // I know, I know. Not necessary. Blame my OCD.
                brick.Connection.Close();
            }
        }
    }
}
