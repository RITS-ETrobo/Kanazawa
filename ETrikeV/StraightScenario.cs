using System;

namespace ETrikeV
{
	public class StraightScenario : Scenario
	{
		private const int LIGHT_WIDTH = 10;
		private int endTachoCount;
		private int speed;
		private Mode edge;

		public StraightScenario (int endTachoCount, int speed, Mode edge)
		{
			this.endTachoCount = endTachoCount;
			this.speed = speed;
			this.edge = edge;
		}

		public override bool run(Ev3System sys)
		{
			// 終了確認
			if ((sys.leftMotorGetTachoCount () + sys.rightMotorGetTachoCount ()) / 2 > endTachoCount) {
				return true;
			}

			// 後輪を制御してライン左エッジを走行する
			// ステアリングは一切変えない
			int light = sys.colorRead ();
			if (edge == Mode.Left) {
				if (light > sys.TargetLight + LIGHT_WIDTH) {
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (speed / 2);
				} else if (light < sys.TargetLight - LIGHT_WIDTH) {
					sys.setLeftMotorPower (speed / 2);
					sys.setRightMotorPower (speed);
				} else {
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (speed);
				}
			} else {
				if (light > sys.TargetLight + LIGHT_WIDTH) {
					sys.setLeftMotorPower (speed / 2);
					sys.setRightMotorPower (speed);
				} else if (light < sys.TargetLight - LIGHT_WIDTH) {
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (speed / 2);
				} else {
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (speed);
				}
			}

			return false;
		}
	}
}

