using System;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.UserInput;

namespace ETrikeV
{
	public class TestScenario : Scenario
	{
		private double kp = 7.5;
		private double ki = 0.0;
		private double kd = 0.0;
		private double freq = 0.004;
		private const double WHEEL_BASE = 212.0;

		private int pos = 0;

		private int[] diff = new int[2];
		private double integral = 0;
		private int speed = 0;

		private bool log = false;

		public TestScenario (int speed)
		{
			this.speed = speed;
			ButtonEvents buts = new ButtonEvents();
			buts.EnterPressed += () => {
				log = true;
			};
			buts.UpPressed += () => {
				switch (pos) {
				case 0:
					kp += 0.1;
					LcdConsole.Clear();
					LcdConsole.WriteLine("kp:" + kp);
					break;
				case 1:
					kd += 0.1;
					LcdConsole.Clear();
					LcdConsole.WriteLine("kd:" + kd);
					break;
				default:
					break;
				}
			};
			buts.DownPressed += () => {
				switch (pos) {
				case 0:
					kp -= 0.1;
					LcdConsole.Clear();
					LcdConsole.WriteLine("kp:" + kp);
					break;
				case 1:
					kd -= 0.1;
					LcdConsole.Clear();
					LcdConsole.WriteLine("kd:" + kd);
					break;
				default:
					break;
				}
			};
			buts.LeftPressed += () => {
				pos = (pos > 0) ? pos - 1 : 0;
			};
			buts.RightPressed += () => {
				pos = (pos < 1) ? pos + 1 : 1;
			};
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
			d = kd * (diff [0] - diff [1]);

			ret = (int)(p + i + d);

			if (log) {
				log = false;
				LcdConsole.Clear ();
				LcdConsole.WriteLine ("diff[0]:" + diff [0] + ", diff[1]:" + diff[1]);
				LcdConsole.WriteLine ("p,i,d:" + p + ", " + i + ", " + d);
				LcdConsole.WriteLine ("PID:" + ret);
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
			int pidVal = pid(sys, light);

			leftP = speed;
			rightP = speed;
			if (pidVal < 0) {
				leftP -= leftP * pidVal * -1 / 100;
			} else if (pidVal > 0) {
				rightP -= rightP * pidVal / 100;
			}

			steerCtrl (sys, leftP, rightP);
			sys.setLeftMotorPower (leftP);
			sys.setRightMotorPower (rightP);

			return false;
		}
	}
}

