using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;

namespace Miss
{
    class MissBootstrapper: DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(
                typeof(IDictionary<char, MotorWrapper>),
                new Dictionary<char, MotorWrapper>()
                {
                    { 'a', new MotorWrapper(null) },
                    { 'b', new MotorWrapper(null) },
                    { 'c', new MotorWrapper(null) },
                    { 'd', new MotorWrapper(null) },
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
            using (var nancyHost =
                       new NancyHost(new MissBootstrapper(), new Uri("http://localhost:11550")))
            {
                nancyHost.Start();
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            }
        }
    }
}
