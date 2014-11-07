using System;
using System.Linq;
using System.Threading;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;

namespace Miss
{
    class MissBootstrapper: DefaultNancyBootstrapper
    {
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
