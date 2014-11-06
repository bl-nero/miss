using System;
using NUnit.Framework;
using Nancy;
using Nancy.Testing;

namespace Miss.Tests {

  [TestFixture]
  public class MotorModuleTest {
    Browser browser;

    [SetUp]
    public void SetUp() {
      browser = new Browser(with => {
        with.Module<MotorModule>();
      });
    }

    [Test]
    public void SwitchOn() {
      var result = browser.Get("/v1/motor/a/switchOn", with => {
        with.HttpRequest();
      });
      Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
  }
}