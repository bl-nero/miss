﻿using System;
using System.Collections.Generic;
using MonoBrick;
using MonoBrick.EV3;
using Nancy;

namespace Miss
{
    public interface IMotor
    {
        void On(sbyte speed);

        void On(sbyte speed, UInt32 degrees);

        void Off();

        void Wait();

        Int32 GetCounter();

        void Reset();

        void TurnTo(byte speed, int position);
    }

    /// <summary>
    /// An implementation of IMotor that is a wrapper over a MonoBrick motor.
    /// </summary>
    public class MotorWrapper: IMotor
    {
        private Motor motor;
        private Connection<Command, Reply> connection;
        private OutputBitfield outputBitfield;

        public MotorWrapper(
            Motor motor, Connection<Command, Reply> connection, OutputBitfield outputBitfield)
        {
            this.motor = motor;
            this.connection = connection;
            this.outputBitfield = outputBitfield;
        }

        public void On(sbyte speed)
        {
            motor.On(speed);
        }

        public void On(sbyte speed, UInt32 degrees)
        {
            // TODO(bl-nero): The speed profile used by MonoBrick by default is stupid. Let's try to
            // use a custom one.
            motor.On(speed, degrees, true /* brake */);
        }

        public void Off()
        {
            motor.Off();
        }

        public void Wait()
        {
            // Note: Unfortunately, the motor's Output property is not public, and
            // Output.WaitForReady isn't exported from the Motor class itself, so we need to make
            // our own one just to call this one method.
            // TODO(bl-nero): This hasn't been properly tested yet!
            Output output = new MissMotorOutput(outputBitfield, motor.DaisyChainLayer, connection);
            output.WaitForReady(true);
        }

        public Int32 GetCounter()
        {
            return motor.GetTachoCount();
        }

        public void Reset()
        {
            motor.ResetTacho();
        }

        public void TurnTo(byte speed, int position)
        {
            motor.MoveTo(speed, position, true /* brake */);
        }
    }

    /// <summary>
    /// This class exists only because the Output.Connection property is internal (gah!). Luckily,
    /// the underlying field is protected. :)
    /// </summary>
    public class MissMotorOutput: Output
    {
        public MissMotorOutput(
            OutputBitfield bf, DaisyChainLayer daisyChainLayer,
            Connection<Command, Reply> connection)
            : base(bf, daisyChainLayer)
        {
            this.connection = connection;
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

        public void On(sbyte speed, UInt32 degrees)
        {
        }

        public void Off()
        {
        }

        public void Wait()
        {
        }

        public Int32 GetCounter()
        {
            Console.Write("Please enter the counter value: ");
            return Int32.Parse(Console.ReadLine());
        }

        public void Reset()
        {
        }

        public void TurnTo(byte speed, int position)
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

            Get[@"/(?<portSpec>^[abcd]$)/turnBy"] = parameters =>
            {
                string portSpec = parameters.portSpec;
                sbyte speed = Request.Query.speed;
                UInt32 degrees = Request.Query.degrees;
                Console.WriteLine(String.Format(
                        "Turning motor {0} by {1} degrees at speed {2}", portSpec, degrees, speed));
                motors[portSpec[0]].On(speed, degrees);
                return HttpStatusCode.OK;
            };

            Get[@"/(?<portSpec>^[abcd]$)/turnTo"] = parameters =>
            {
                string portSpec = parameters.portSpec;
                byte speed = Request.Query.speed;
                int degrees = Request.Query.degrees;
                Console.WriteLine(String.Format(
                        "Turning motor {0} to position of {1} degrees at speed {2}",
                        portSpec, degrees, speed));
                motors[portSpec[0]].TurnTo(speed, degrees);
                return HttpStatusCode.OK;
            };

            Get[@"/(?<portSpec>^[abcd]$)/wait"] = parameters =>
            {
                string portSpec = parameters.portSpec;
                Console.WriteLine(String.Format("Waiting for motor {0}", portSpec));
                motors[portSpec[0]].Wait();
                return HttpStatusCode.OK;
            };

            Get[@"/(?<portSpec>^[abcd]$)/getCounter"] = parameters =>
            {
                string portSpec = parameters.portSpec;
                Console.WriteLine(String.Format("Getting counter of motor {0}", portSpec));
                return motors[portSpec[0]].GetCounter().ToString();
            };

            Get[@"/(?<portSpec>^[abcd]$)/reset"] = parameters =>
            {
                string portSpec = parameters.portSpec;
                Console.WriteLine(String.Format("Resetting counter of motor {0}", portSpec));
                motors[portSpec[0]].Reset();
                return HttpStatusCode.OK;
            };
        }
    }
}