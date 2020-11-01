using MBN.Enums;
using MBN.Modules;
using Microsoft.SPOT;
using MBN;
using System.Threading;

namespace Examples
{
    public class Program
    {
        static DCMotorClick _motor;

        public static void Main()
        {
            _motor = new DCMotorClick(Hardware.SocketOne);

            _motor.OnFault += Motor_OnFault;

            _motor.Move(DCMotorClick.Directions.Forward, 1.0, 2000);
            Thread.Sleep(5000);
            _motor.Stop(2000);

            while (_motor.IsMoving) { Thread.Sleep(10); }

            Debug.Print("Sleep");
            _motor.PowerMode = PowerModes.Low;

            Debug.Print("Move");
            _motor.Move(DCMotorClick.Directions.Backward, 0.75);
            Thread.Sleep(2000);
            _motor.Stop();


            Debug.Print("Wake up");
            _motor.PowerMode = PowerModes.On;

            Debug.Print("Move");
            _motor.Move(DCMotorClick.Directions.Backward);
            Thread.Sleep(3000);
            _motor.Stop();
        }

        static void Motor_OnFault(object sender, EventArgs e)
        {
            Debug.Print("Over current protection !!!");
        }
    }
}
