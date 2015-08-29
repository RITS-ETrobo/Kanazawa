using System;
using System.Threading;
using MonoBrickFirmware.UserInput;

namespace ETrikeV
{
	public class TestScenario : Scenario
	{
		private int outSpeed = 100;
		private int inSpeed = 0;
		private int turn = 0;
		private bool end = false;

		public TestScenario ()
		{
			ButtonEvents buts = new ButtonEvents();
			buts.UpPressed += () => {
				inSpeed += 10;
			};
			buts.DownPressed += () => {
				inSpeed -= 10;
			};
			buts.EnterPressed += () => {
				end = true;
			};
			buts.LeftPressed += () => {
				turn -= 10;
			};
			buts.RightPressed += () => {
				turn += 10;
			};
		}

		public override bool run(Ev3System sys)
		{
			int current = turn;
			sys.setSteerSlope (current);
			while (!end) {
				if (current < 0) {
					sys.setLeftMotorPower (inSpeed);
					sys.setRightMotorPower (outSpeed);
				} else {
					sys.setLeftMotorPower (outSpeed);
					sys.setRightMotorPower (inSpeed);
				}
			}

			sys.stopMotors ();
			end = false;
			Thread.Sleep (1000);

			return false;
		}
	}
}

