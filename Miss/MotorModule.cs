using System;
using System.Collections.Generic;
using MonoBrick.EV3;
using Nancy;

namespace Miss
{
    /// <summary>
    /// A simple wrapper for MonoBrick motor object, for easier mocking.
    /// </summary>
    public class MotorWrapper
    {
        private Motor motor;

        public MotorWrapper(Motor motor)
        {
            this.motor = motor;
        }

        public virtual void On(sbyte speed)
        {
            //motor.On(speed);
        }
    }

    /// <summary>
    /// A module that is responsible for interacting with motors. It handles URLs of a following
    /// structure: <c>/v1/motor/[portSpec]/[action]</c>, where <c>[portSpec]</c> decides which motor
    /// (or pair of motors) will be used, and </c>[action]</c> is the name of action to be
    /// performed.
    /// </summary>
    public class MotorModule: NancyModule
    {
        public MotorModule(IDictionary<char, MotorWrapper> motors)
            : base("/v1/motor")
        {
            Get["/{portSpec}/switchOn"] = parameters =>
            {
                sbyte speed = Request.Query.speed;
                string portSpec = parameters.portSpec;
                motors[portSpec[0]].On(speed);
                Console.WriteLine(
                    "Switching on motor " + parameters.portSpec + " to speed " + speed);
                return HttpStatusCode.OK;
            };
        }
    }
}