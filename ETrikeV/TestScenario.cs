using System;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.UserInput;

namespace ETrikeV
{
	public class TestScenario : Scenario
	{
		public TestScenario ()
		{
			
		}

		public override bool run(Ev3System sys)
		{
			int startTachoCount = 0;
			int currentTachoCount = 0;
			motorA = new Motor(MotorPort.OutA);
			motorB = new Motor(MotorPort.OutB);
			motorC = new Motor(MotorPort.OutC);
			motorD = new Motor(MotorPort.OutD);
			
			startTachoCount = motorA.GetTachoCount();
			motorA.SetPower(100);
			while (true) {
				currentTachoCount = motorA.GetTachoCount();
				if (currentTachoCount > startTachoCount + 5000) {
					break;
				}
				motorA.Brake();
				LcdConsole.WriteLine("MotorA : ");
			}
			
			startTachoCount = motorB.GetTachoCount();
			motorB.SetPower(100);
			while (true) {
				currentTachoCount = motorB.GetTachoCount();
				if (currentTachoCount > startTachoCount + 5000) {
					break;
				}
				motorB.Brake();
				LcdConsole.WriteLine("MotorB : ");
			}
			
			startTachoCount = motorC.GetTachoCount();
			motorC.SetPower(100);
			while (true) {
				currentTachoCount = motorC.GetTachoCount();
				if (currentTachoCount > startTachoCount + 5000) {
					break;
				}
				motorC.Brake();
				LcdConsole.WriteLine("MotorC : ");
			}
			
			startTachoCount = motorD.GetTachoCount();
			motorD.SetPower(100);
			while (true) {
				currentTachoCount = motorD.GetTachoCount();
				if (currentTachoCount > startTachoCount + 5000) {
					break;
				}
				motorD.Brake();
				LcdConsole.WriteLine("MotorD : ");
			}
			
			Thred.Sleep(10000);
			return false;
		}
	}
}

