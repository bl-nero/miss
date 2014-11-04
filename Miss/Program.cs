using Nancy.Hosting.Self;
using System;
using System.Threading;

namespace Miss {
  class MainClass {
    public static void Main(string[] args) {
      using (var nancyHost = new NancyHost(new Uri("http://localhost:11550"))) {
        nancyHost.Start();
        System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
      }
    }
  }
}
