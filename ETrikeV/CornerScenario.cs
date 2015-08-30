using System;

namespace ETrikeV
{
	public class CornerScenario : Scenario
	{
		private const int SPEED_WIDTH = 20;
		private const int LIGHT_WIDTH = 5; //10
		private int endTachoCount;
		private int inSpeed;
		private int outSpeed;
		private int direction;
		private Mode edge;
		private bool init = false;

		public CornerScenario (int endTachoCount, int inSpeed, int outSpeed, int direction, Mode edge)
		{
			this.endTachoCount = endTachoCount;
			this.inSpeed = inSpeed;
			this.outSpeed = outSpeed;
			this.direction = direction;
			this.edge = edge;
		}

		public override bool run(Ev3System sys)
		{
			// 終了確認
			if (sys.getAverageTachoCount() > endTachoCount) {
				sys.setSteerSlope (0);
				return true;
			}

			// 前輪の回転（初回のみ）
			if (!init) {
				sys.setSteerSlope (direction);
				init = true;
			}

			int leftMotorPwr, rightMotorPwr;
			int light = sys.colorRead ();
			if (direction <= 0) { // 左カーブ
				leftMotorPwr = inSpeed;
				rightMotorPwr = outSpeed;
				if (edge == Mode.Left) {
					if (light > sys.TargetLight + LIGHT_WIDTH) {
						leftMotorPwr = rightMotorPwr;
					} else if (light < sys.TargetLight - LIGHT_WIDTH) {
						leftMotorPwr = 0;
					}
				} else {
					if (light > sys.TargetLight + LIGHT_WIDTH) {
						leftMotorPwr = 0;
					} else if (light < sys.TargetLight - LIGHT_WIDTH) {
						leftMotorPwr = rightMotorPwr;
					}
				}
			} else { // 右カーブ
				leftMotorPwr = outSpeed;
				rightMotorPwr = inSpeed;
				if (edge == Mode.Left) {
					if (light > sys.TargetLight + LIGHT_WIDTH) {
						rightMotorPwr = 0;
					} else if (light < sys.TargetLight - LIGHT_WIDTH) {
						rightMotorPwr = leftMotorPwr;
					}
				} else {
					if (light > sys.TargetLight + LIGHT_WIDTH) {
						rightMotorPwr = leftMotorPwr;
					} else if (light < sys.TargetLight - LIGHT_WIDTH) {
						rightMotorPwr = 0;
					}
				}
			}

			sys.setLeftMotorPower (leftMotorPwr);
			sys.setRightMotorPower (rightMotorPwr);

			return false;
		}
	}
}

