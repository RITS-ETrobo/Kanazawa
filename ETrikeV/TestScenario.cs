using System;

namespace ETrikeV
{
	public class TestScenario : Scenario
	{
		private double kp = 0.0;
		private double ki = 0.0;
		private double kd = 0.0;

		private int[] diff = new int[2];
		private double integral = 0;
		private int speed = 0;

		public TestScenario (int speed)
		{
			this.speed = speed;
		}

		private int pd(Ev3System sys, int light)
		{
			int ret = 0;
			double p, i, d;

			diff [1] = diff [0];
			diff [0] = light - sys.TargetLight;
			integral += (diff [0] + diff [1]) / 2 * 0.005;

			p = kp * diff [0];
			i = ki * integral;
			d = kd * (diff [0] - diff [1]) / 0.005;

			ret = (int)(p + i + d);
			if (ret > 100)
				ret = 100;
			if (ret < -100)
				ret = -100;

			return ret;
		}

		private void steerCtrl(Ev3System sys, int leftPwr, int rightPwr)
		{
			int steerPwr = 100;
			int diffP = leftPwr - rightPwr;
			int target = diffP * 18 / 10;
			int current = sys.steerGetTachoCount ();

			// max値付近の場合は止める
			if (Math.Abs(target - current) < 3) {
				sys.steerBrake ();
				return;
			}

			if (Math.Abs (target - current) < 50) {
				steerPwr = 50;
			}
			if (current < target) {
				sys.setSteerPower (steerPwr);
			} else {
				sys.setSteerPower (-1 * steerPwr);
			}

			return;
		}

		public override bool run(Ev3System sys)
		{
			int leftP, rightP;
			int light = sys.colorRead ();
			int pdVal = pd(sys, light);

			leftP = speed;
			rightP = speed;
			if (pdVal < 0) {
				leftP -= leftP * pdVal * -1 / 100;
			} else if (pdVal > 0) {
				rightP -= rightP * pdVal / 100;
			}

			steerCtrl (sys, leftP, rightP);

			return false;
		}
	}
}

