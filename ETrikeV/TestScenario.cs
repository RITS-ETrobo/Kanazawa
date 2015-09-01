using System;
using MonoBrickFirmware.Display;

namespace ETrikeV
{
	public class TestScenario : Scenario
	{
		private double kp = 4.5;
		private double ki = 0.0;
		private double kd = 8.0;
		private double freq = 0.004;
		private int cnt = 0;
		private const double WHEEL_BASE = 212.0;

		private int[] diff = new int[2];
		private double integral = 0;
		private int speed = 0;

		public TestScenario (int speed)
		{
			this.speed = speed;
		}

		private int pid(Ev3System sys, int light)
		{
			int ret = 0;
			double p, i, d;

			diff [1] = diff [0];
			diff [0] = light - sys.TargetLight;
			integral += (diff [0] + diff [1]) / 2 * freq;

			p = kp * diff [0];
			i = ki * integral;
			d = kd * (diff [0] - diff [1]) / freq;

			ret = (int)(p + i + d);

			if (cnt++ % 120 == 0) {
				LcdConsole.Clear ();
				LcdConsole.WriteLine ("PID : " + ret);
			}
			ret = (ret > 100) ? 100 : ret;
			ret = (ret < -100) ? -100 : ret;

			return ret;
		}

		private void steerCtrl(Ev3System sys, int leftPwr, int rightPwr)
		{
			int target = 0;
			if (leftPwr != rightPwr) {
				double r = (rightPwr + leftPwr) / (rightPwr - leftPwr) * (121 / 2);
				target = (int)(Math.Atan (WHEEL_BASE / r) * 180 / Math.PI * 8 * -1);
			}
			int current = sys.steerGetTachoCount ();
			int diff = (target - current) * 2;
			diff = (diff > 100) ? 100 : diff;
			diff = (diff < -100) ? -100 : diff;
			sys.setSteerPower (diff);

			return;
		}

		public override bool run(Ev3System sys)
		{
			int leftP, rightP;
			int light = sys.colorRead ();
			int pdVal = pid(sys, light);

			leftP = speed;
			rightP = speed;
			if (pdVal < 0) {
				leftP -= leftP * pdVal * -1 / 100;
			} else if (pdVal > 0) {
				rightP -= rightP * pdVal / 100;
			}

			steerCtrl (sys, leftP, rightP);
			sys.setLeftMotorPower (leftP);
			sys.setRightMotorPower (rightP);

			return false;
		}
	}
}

