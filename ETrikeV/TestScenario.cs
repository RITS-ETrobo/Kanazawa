using System;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.UserInput;
using MonoBrickFirmware.Movement;
using System.Diagnostics;

namespace ETrikeV
{
	public class TestScenario : Scenario
	{
		Motor motorA = new Motor(MotorPort.OutA);
		Motor motorB = new Motor(MotorPort.OutB);
//		Motor motorC = new Motor(MotorPort.OutC);
		//			Motor motorD = new Motor(MotorPort.OutD);

		Stopwatch sw = new Stopwatch ();
		bool flag = false;

		int count = 5000;

		public TestScenario ()
		{
			ButtonEvents buts = new ButtonEvents();
			buts.EnterPressed += () =>
			{ 
				LcdConsole.Clear();
				flag = true;
			};
		}

		public override bool run(Ev3System sys)
		{
			int startTachoCount = 0;
			int currentTachoCount = 0;

			motorA.ResetTacho ();
			startTachoCount = motorA.GetTachoCount();
			sw.Reset ();
			sw.Start ();
			motorA.SetPower(100);
			while (true) {
				currentTachoCount = motorA.GetTachoCount ();
				if (currentTachoCount > startTachoCount + count) {
					sw.Stop ();
					break;
				}
			}
			motorA.Brake();
			LcdConsole.WriteLine("MotorA : " + sw.ElapsedMilliseconds);

			motorB.ResetTacho ();
			startTachoCount = motorB.GetTachoCount();
			sw.Reset ();
			sw.Start ();
			motorB.SetPower(100);
			while (true) {
				currentTachoCount = motorB.GetTachoCount ();
				if (currentTachoCount > startTachoCount + count) {
					sw.Stop ();
					break;
				}
			}
			motorB.Brake();
			LcdConsole.WriteLine("MotorB : " + sw.ElapsedMilliseconds);

//			motorC.ResetTacho ();
//			startTachoCount = motorC.GetTachoCount();
//			sw.Reset ();
//			sw.Start ();
//			motorC.SetPower(100);
//			while (true) {
//				currentTachoCount = motorC.GetTachoCount ();
//				if (currentTachoCount > startTachoCount + count) {
//					sw.Stop ();
//					break;
//				}
//			}
//			motorC.Brake();
//			LcdConsole.WriteLine("MotorC : " + sw.ElapsedMilliseconds);
			
//			startTachoCount = motorD.GetTachoCount();
//			motorD.SetPower(100);
//			while (true) {
//				currentTachoCount = motorD.GetTachoCount();
//				if (currentTachoCount > startTachoCount + count) {
//					break;
//				}
//				motorD.Brake();
//				LcdConsole.WriteLine("MotorD : ");
//			}
			
			while (!flag) {
				System.Threading.Thread.Sleep (100);
			}
			flag = false;

			return false;
		}
	}
}

