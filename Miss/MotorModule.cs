using System;
using System.Collections.Generic;
using MonoBrick.EV3;
using Nancy;

namespace Miss
{
    public interface IMotor
    {
        void On(sbyte speed);

        void Off();
    }

    /// <summary>
    /// An implementation of IMotor that is a wrapper over a MonoBrick motor.
    /// </summary>
    public class MotorWrapper: IMotor
    {
        private Motor motor;

        public MotorWrapper(Motor motor)
        {
            this.motor = motor;
        }

        public void On(sbyte speed)
        {
            motor.On(speed);
        }

        public void Off()
        {
            motor.Off();
        }
    }

    /// <summary>
    /// An IMotor implementation that is just a dummy for testing purposes.
    /// </summary>
    public class DummyMotorWrapper: IMotor
    {
        public void On(sbyte speed)
        {
        }

        public void Off()
        {
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
        public MotorModule(IDictionary<char, IMotor> motors)
            : base("/v1/motor")
        {
            Get[@"/(?<portSpec>^[abcd]$)/switchOn"] = parameters =>
            {
                sbyte speed = Request.Query.speed;
                string portSpec = parameters.portSpec;
                Console.WriteLine(String.Format(
                        "Switching on motor {0} to speed {1}", parameters.portSpec, speed));
                motors[portSpec[0]].On(speed);
                return HttpStatusCode.OK;
            };

            Get[@"/(?<portSpec>^[abcd]$)/switchOff"] = parameters =>
            {
                string portSpec = parameters.portSpec;
                Console.WriteLine(String.Format("Switching off motor {0}", portSpec));
                motors[portSpec[0]].Off();
                return HttpStatusCode.OK;
            };
        }
    }
}