using System;
using System.Collections.Generic;
using System.Linq;
using MonoBrick.EV3;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Miss
{
    /// <summary>
    /// A base class for MISS server bootstrappers.
    /// </summary>
    class MissBootstrapperBase: DefaultNancyBootstrapper
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

            pipelines.OnError += (ctx, ex) =>
            {
                Console.Error.WriteLine(ex);
                return null;
            };
        }
    }

    /// <summary>
    /// A MISS bootstrapper that connects to an EV3 brick.
    /// </summary>
    class DefaultMissBootstrapper: MissBootstrapperBase
    {
        private string brickConnection;

        public DefaultMissBootstrapper(string brickConnection)
        {
            this.brickConnection = brickConnection;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            Console.Error.WriteLine(
                String.Format("Connecting to the brick at {0}...", brickConnection));
            var brick = new Brick<Sensor, Sensor, Sensor, Sensor>(brickConnection);
            brick.Connection.Open();
            // Register a connection guard that will close the connection once the application
            // container is disposed. This is probably an overkill, but I wouldn't lay my head to
            // sleep peacefully otherwise.
            container.Register(new ConnectionGuard(brick.Connection));
            Console.Error.WriteLine("Connection established");
            container.Register(
                typeof(IDictionary<char, IMotor>),
                new Dictionary<char, IMotor>()
                {
                    { 'a', new MotorWrapper(brick.MotorA) },
                    { 'b', new MotorWrapper(brick.MotorB) },
                    { 'c', new MotorWrapper(brick.MotorC) },
                    { 'd', new MotorWrapper(brick.MotorD) },
                });
        }
    }

    /// <summary>
    /// A MISS bootstrapper that only pretends to connect to anything, but uses dummy classes
    /// instead. This is used for testing purposes.
    /// </summary>
    class DummyMissBootstrapper: MissBootstrapperBase
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            Console.Error.WriteLine("Using dummy connection");
            container.Register(
                typeof(IDictionary<char, IMotor>),
                new[]{ 'a', 'b', 'c', 'd' }.ToDictionary(
                    name => name,
                    _ => (IMotor)new DummyMotorWrapper()));
        }
    }

    /// <summary>
    /// A handy class that makes sure that a MonoBrick connection gets properly disposed.
    /// </summary>
    public class ConnectionGuard: IDisposable
    {
        private MonoBrick.Connection<Command, Reply> connection;

        public ConnectionGuard(MonoBrick.Connection<Command, Reply> connection)
        {
            this.connection = connection;
        }

        public void Dispose()
        {
            connection.Close();
            Console.Error.WriteLine("Connection closed.");
        }
    }
}