using System;
using System.Collections.Generic;
using System.Linq;
using MonoBrick.EV3;
using Moq;
using NUnit.Framework;
using Nancy;
using Nancy.Testing;

namespace Miss.Tests
{
    [TestFixture]
    public class MotorModuleTest
    {
        MockRepository mocks;
        Dictionary<char, Mock<IMotor>> motorMocks;
        Browser browser;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository(MockBehavior.Strict);
            motorMocks = new[]{ 'a', 'b', 'c', 'd' }.ToDictionary(
                name => name, name => mocks.Create<IMotor>());
            var motors = motorMocks.ToDictionary(item => item.Key, item => item.Value.Object);
            browser = new Browser(with =>
                {
                    with.Module<MotorModule>();
                    with.Dependency(motors);
                });
        }

        [TearDown]
        public void TearDown()
        {
            mocks.Verify();
        }

        [Test]
        public void SwitchOn()
        {
            motorMocks['a'].Setup(motor => motor.On(50)).Verifiable();
            motorMocks['b'].Setup(motor => motor.On(60)).Verifiable();
            BrowserResponse responseA =
                browser.Get("/v1/motor/a/switchOn", with => with.Query("speed", "50"));
            BrowserResponse responseB =
                browser.Get("/v1/motor/b/switchOn", with => with.Query("speed", "60"));
            Assert.That(responseA.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseB.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void SwitchOff()
        {
            motorMocks['a'].Setup(motor => motor.Off()).Verifiable();
            motorMocks['b'].Setup(motor => motor.Off()).Verifiable();
            BrowserResponse responseA = browser.Get("/v1/motor/a/switchOff");
            BrowserResponse responseB = browser.Get("/v1/motor/b/switchOff");
            Assert.That(responseA.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseB.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void TurnBy()
        {
            motorMocks['a'].Setup(motor => motor.On(50, 180, false /* brake */)).Verifiable();
            motorMocks['b'].Setup(motor => motor.On(100, 90, true /* brake */)).Verifiable();
            BrowserResponse responseA = browser.Get("/v1/motor/a/turnBy", with =>
                {
                    with.Query("speed", "50");
                    with.Query("degrees", "180");
                    with.Query("brake", "false");
                });
            BrowserResponse responseB = browser.Get("/v1/motor/b/turnBy", with =>
                {
                    with.Query("speed", "100");
                    with.Query("degrees", "90");
                    with.Query("brake", "true");
                });
            Assert.That(responseA.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseB.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void TurnByWithoutBrakeParameter()
        {
            motorMocks['a'].Setup(motor => motor.On(50, 180, false /* brake */)).Verifiable();
            BrowserResponse responseA = browser.Get("/v1/motor/a/turnBy", with =>
                {
                    with.Query("speed", "50");
                    with.Query("degrees", "180");
                });
            Assert.That(responseA.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void TurnToForward()
        {
            motorMocks['a'].Setup(motor => motor.GetCounter()).Returns(100);
            motorMocks['a'].Setup(motor => motor.On(10, 80, false /* brake */)).Verifiable();
            BrowserResponse response = browser.Get("/v1/motor/a/turnTo", with =>
                {
                    with.Query("speed", "10");
                    with.Query("degrees", "180");
                });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void TurnToForwardFromNegativeCounter()
        {
            motorMocks['b'].Setup(motor => motor.GetCounter()).Returns(-200);
            motorMocks['b'].Setup(motor => motor.On(100, 290, false /* brake */)).Verifiable();
            BrowserResponse response = browser.Get("/v1/motor/b/turnTo", with =>
                {
                    with.Query("speed", "100");
                    with.Query("degrees", "90");
                });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void TurnToBackwards()
        {
            motorMocks['c'].Setup(motor => motor.GetCounter()).Returns(180);
            motorMocks['c'].Setup(motor => motor.On(-10, 80, false /* brake */)).Verifiable();
            BrowserResponse response = browser.Get("/v1/motor/c/turnTo", with =>
                {
                    with.Query("speed", "10");
                    with.Query("degrees", "100");
                });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void TurnToBackwardsToNegativeCounter()
        {
            motorMocks['d'].Setup(motor => motor.GetCounter()).Returns(90);
            motorMocks['d'].Setup(motor => motor.On(-100, 290, false /* brake */)).Verifiable();
            BrowserResponse response = browser.Get("/v1/motor/d/turnTo", with =>
                {
                    with.Query("speed", "100");
                    with.Query("degrees", "-200");
                });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void TurnToBraking()
        {
            motorMocks['a'].Setup(motor => motor.GetCounter()).Returns(0);
            motorMocks['a'].Setup(motor => motor.On(10, 90, false /* brake */)).Verifiable();
            motorMocks['b'].Setup(motor => motor.GetCounter()).Returns(0);
            motorMocks['b'].Setup(motor => motor.On(10, 90, true /* brake */)).Verifiable();
            BrowserResponse responseA = browser.Get("/v1/motor/a/turnTo", with =>
                {
                    with.Query("speed", "10");
                    with.Query("degrees", "90");
                    with.Query("brake", "false");
                });
            BrowserResponse responseB = browser.Get("/v1/motor/b/turnTo", with =>
                {
                    with.Query("speed", "10");
                    with.Query("degrees", "90");
                    with.Query("brake", "true");
                });
            Assert.That(responseA.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseB.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void Brake()
        {
            motorMocks['a'].Setup(motor => motor.Brake()).Verifiable();
            Assert.That(browser.Get("/v1/motor/a/brake").StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void Wait()
        {
            motorMocks['a'].Setup(motor => motor.Wait()).Verifiable();
            Assert.That(browser.Get("/v1/motor/a/wait").StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void GetCounter()
        {
            motorMocks['a'].Setup(motor => motor.GetCounter()).Returns(10);
            motorMocks['b'].Setup(motor => motor.GetCounter()).Returns(20);
            BrowserResponse responseA = browser.Get("/v1/motor/a/getCounter");
            BrowserResponse responseB = browser.Get("/v1/motor/b/getCounter");
            Assert.That(responseA.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseB.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseA.Body.AsString(), Is.EqualTo("10"));
            Assert.That(responseB.Body.AsString(), Is.EqualTo("20"));
        }

        [Test]
        public void Reset()
        {
            motorMocks['a'].Setup(motor => motor.Reset()).Verifiable();
            Assert.That(browser.Get("/v1/motor/a/reset").StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void IllegalPortNames()
        {
            Action<BrowserContext> withQuery = with => with.Query("speed", "50");
            BrowserResponse response = browser.Get("/v1/motor/foo/switchOn", withQuery);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            response = browser.Get("/v1/motor/e/switchOn", withQuery);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}