using System;

namespace ETrikeV
{
	public class LineTraceScenario : Scenario
	{
		private const int MAX_STEERING_ANGLE = 180;
		private const int STEER_POWER = 100;
		private const int LIGHT_WIDTH = 10;
		private int endTachoCount;
		private int speed;
		private Mode edge;

		public LineTraceScenario (int endTachoCount, int speed, Mode edge)
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

			int light = sys.colorRead ();
			int steerCnt = sys.steerGetTachoCount ();
			if (edge == Mode.Left) {
				if (light > sys.TargetLight + LIGHT_WIDTH) {
					// 白い場合は右に曲がる
					if (steerCnt < MAX_STEERING_ANGLE) {
						sys.setSteerPower (STEER_POWER);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (0);
				} else if (light < sys.TargetLight - LIGHT_WIDTH) {
					// 黒い場合は左に曲がる
					if (steerCnt > -1 * MAX_STEERING_ANGLE) {
						sys.setSteerPower (-1 * STEER_POWER);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (0);
					sys.setRightMotorPower (speed);
				} else {
					// 灰色から規定範囲内ならステアリングをとめてゆっくり進む
					sys.steerBrake ();
					sys.setLeftMotorPower (speed / 2);
					sys.setRightMotorPower (speed / 2);
				}
			} else {
				if (light > sys.TargetLight + LIGHT_WIDTH) {
					// 白い場合は左に曲がる
					if (steerCnt > -1 * MAX_STEERING_ANGLE) {
						sys.setSteerPower (-1 * STEER_POWER);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (0);
					sys.setRightMotorPower (speed);
				} else if (light < sys.TargetLight - LIGHT_WIDTH) {
					// 黒い場合は右に曲がる
					if (steerCnt < MAX_STEERING_ANGLE) {
						sys.setSteerPower (STEER_POWER);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (0);
				} else {
					// 灰色から規定範囲内ならステアリングをとめてゆっくり進む
					sys.steerBrake ();
					sys.setLeftMotorPower (speed / 2);
					sys.setRightMotorPower (speed / 2);
				}
			}

			return false;
		}
	}
}

