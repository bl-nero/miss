using System;
using Nancy;

namespace Miss {

  /// <summary>
  /// A module that is responsible for interacting with motors. It handles URLs
  /// of a following structure: <c>/v1/motor/[portSpec]/[action]</c>, where
  /// <c>[portSpec]</c> decides which motor (or pair of motors) will be used,
  /// and </c>[action]</c> is the name of action to be performed.
  /// </summary>
  public class MotorModule: NancyModule {

    public MotorModule() : base("/v1/motor") {
      Get["/{portSpec}/switchOn"] = parameters => {
        Console.WriteLine("Switching on motor " + parameters.portSpec);
        return "Switching on motor " + parameters.portSpec;
      };
    }
  }
}