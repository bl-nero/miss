using System;
using Nancy;

namespace Miss {

 public class MotorModule: NancyModule {

    public MotorModule() : base("/v1/motor") {
      Get["/{portSpec}/switchOn"] = parameters => {
        return "Switching on motor " + parameters.portSpec;
      };
    }
  }
}