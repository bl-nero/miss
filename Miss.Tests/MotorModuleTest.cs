﻿using System;
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
        Dictionary<char, Mock<MotorWrapper>> motorMocks;
        Browser browser;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository(MockBehavior.Loose);
            motorMocks = new[]{ 'a', 'b', 'c', 'd' }.ToDictionary(
                name => name, name => mocks.Create<MotorWrapper>(null));
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