using System;

namespace ETrikeV
{
	public class CornerScenario : Scenario
	{
		private const int LIGHT_WIDTH = 10;
		private int endTachoCount;
		private int speed;
		private int direction;
		private Mode edge;
		private bool init = false;

		public CornerScenario (int endTachoCount, int speed, int direction, Mode edge)
		{
			this.endTachoCount = endTachoCount;
			this.speed = speed;
			this.direction = direction;
			this.edge = edge;
		}

		public override bool run(Ev3System sys)
		{
			// 終了確認
			if ((sys.leftMotorGetTachoCount () + sys.rightMotorGetTachoCount ()) / 2 > endTachoCount) {
				sys.setSteerSlope (0);
				return true;
			}

			// 前輪の回転（初回のみ）
			if (!init) {
				sys.setSteerSlope (direction);
				init = true;
			}

			int light = sys.colorRead ();
			if (direction <= 0) { // 左カーブ
				if (edge == Mode.Left) {
					if (light > sys.TargetLight + LIGHT_WIDTH) {
						sys.setLeftMotorPower (speed / 2);
						sys.setRightMotorPower (speed / 4);
					} else if (light < sys.TargetLight - LIGHT_WIDTH) {
						sys.setLeftMotorPower (speed / 5);
						sys.setRightMotorPower (speed);
					} else {
						sys.setLeftMotorPower (speed / 4);
						sys.setRightMotorPower (speed / 4);
					}
				} else {
					if (light > sys.TargetLight + LIGHT_WIDTH) {
						sys.setLeftMotorPower (speed / 5);
						sys.setRightMotorPower (speed);
					} else if (light < sys.TargetLight - LIGHT_WIDTH) {
						sys.setLeftMotorPower (speed / 2);
						sys.setRightMotorPower (speed / 4);
					} else {
						sys.setLeftMotorPower (speed / 4);
						sys.setRightMotorPower (speed / 4);
					}
				}
			} else { // 右カーブ
				if (edge == Mode.Left) {
					if (light > sys.TargetLight + LIGHT_WIDTH) {
						sys.setLeftMotorPower (speed);
						sys.setRightMotorPower (speed / 5);
					} else if (light < sys.TargetLight - LIGHT_WIDTH) {
						sys.setLeftMotorPower (speed / 4);
						sys.setRightMotorPower (speed / 2);
					} else {
						sys.setLeftMotorPower (speed / 4);
						sys.setRightMotorPower (speed / 4);
					}
				} else {
					if (light > sys.TargetLight + LIGHT_WIDTH) {
						sys.setLeftMotorPower (speed / 4);
						sys.setRightMotorPower (speed / 2);
					} else if (light < sys.TargetLight - LIGHT_WIDTH) {
						sys.setLeftMotorPower (speed);
						sys.setRightMotorPower (speed / 5);
					} else {
						sys.setLeftMotorPower (speed / 4);
						sys.setRightMotorPower (speed / 4);
					}
				}
			}

			return false;
		}
	}
}

