using System;
using System.Threading;

namespace ETrikeV
{
	public class BridgeEscapeScenario : Scenario
	{
		private enum BridgeEscapeState {Init, Turn1, Straight1, Turn2, Straight2, Turn3, Straight3, Turn4, Line, End};
		private BridgeEscapeState st = BridgeEscapeState.Init;
		private Mode edge;
		private Mode escapeDir;
		private const int MOTOR_POWER = 50;
		private const int TURN_MOVE_TACHO = 290;

		public BridgeEscapeScenario (Mode edge, Mode escapeDir)
		{
			this.edge = edge;
			this.escapeDir = escapeDir;
		}

		public override bool run(Ev3System sys)
		{
			bool ret = false;
			Mode turnDir = Mode.Left;

			switch (st) {
			case BridgeEscapeState.Init:
				sys.stopMotors ();
				st = BridgeEscapeState.Turn1;
				break;
			case BridgeEscapeState.Turn1:
				turnDir = (escapeDir == Mode.Left) ? Mode.Left : Mode.Right;
				turn (sys, turnDir, 0);
				st = BridgeEscapeState.Straight1;
				break;
			case BridgeEscapeState.Straight1:
				actionStraight (sys, 10, MOTOR_POWER);
				st = BridgeEscapeState.Turn2;
				break;
			case BridgeEscapeState.Turn2:
				turnDir = (escapeDir == Mode.Left) ? Mode.Right : Mode.Left;
				turn (sys, turnDir, 0);
				st = BridgeEscapeState.Straight2;
				break;
			case BridgeEscapeState.Straight2:
				actionStraight (sys, 40, MOTOR_POWER);
				st = BridgeEscapeState.Turn3;
				break;
			case BridgeEscapeState.Turn3:
				turnDir = (escapeDir == Mode.Left) ? Mode.Right : Mode.Left;
				turn (sys, turnDir, 0);
				st = BridgeEscapeState.Straight3;
				break;
			case BridgeEscapeState.Straight3:
				actionStraight (sys, 10, MOTOR_POWER);
				st = BridgeEscapeState.Turn4;
				break;
			case BridgeEscapeState.Turn4:
				turnDir = (escapeDir == Mode.Left) ? Mode.Left : Mode.Right;
				turn (sys, turnDir, 0);
				st = BridgeEscapeState.Line;
				break;
			case BridgeEscapeState.Line:
				line (sys);
				st = BridgeEscapeState.End;
				break;
			case BridgeEscapeState.End:
				ret = true;
				break;
			default:
				break;
			}

			return ret;
		}

		/// <summary>
		/// 90度回転する
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="turnDir">Turn dir.</param>
		private void turn(Ev3System sys, Mode turnDir, int offset)
		{
			int lPwr, rPwr, angle, sTacho, cTacho;

			// 安定するまで止まる
			Thread.Sleep (200);

			// 駆動輪のパワーを設定
			if (turnDir == Mode.Left) {
				lPwr = 0;
				rPwr = MOTOR_POWER;
				sTacho = sys.rightMotorGetTachoCount ();
			} else {
				lPwr = MOTOR_POWER;
				rPwr = 0;
				sTacho = sys.leftMotorGetTachoCount ();
			}

			// 前輪を回転させる
			angle = SteerCtrl.getSteeringAngle (lPwr, rPwr);
			sys.setSteerSlope (angle);

			// 動作開始
			if (turnDir == Mode.Left) {
				sys.leftMotorBrake ();
				sys.setRightMotorPower (rPwr);
			} else {
				sys.setLeftMotorPower (lPwr);
				sys.rightMotorBrake ();
			}

			// 90度曲がるまでループ
			cTacho = 0;
			while (cTacho < sTacho + TURN_MOVE_TACHO + offset) {
				Thread.Sleep (1);
				if (turnDir == Mode.Left) {
					cTacho = sys.rightMotorGetTachoCount ();
				} else {
					cTacho = sys.leftMotorGetTachoCount ();
				}
			}

			// 停止して終了
			sys.stopMotors ();
			sys.setSteerSlope (0);
		}

		/// <summary>
		/// ライントレース
		/// </summary>
		/// <param name="sys">Sys.</param>
		private void line(Ev3System sys)
		{
			// ラインを探す
			serchLine (sys, 3, false, Mode.Right);

			int cur = 0;
			int cm = sys.getAverageMoveCM ();

			while (cur < cm + 10) {
				lineTrace (sys, MOTOR_POWER, edge, 5, 160, 100);
				cur = sys.getAverageMoveCM ();
			}

			sys.stopMotors ();
			sys.setSteerSlope (0);
		}
	}
}

