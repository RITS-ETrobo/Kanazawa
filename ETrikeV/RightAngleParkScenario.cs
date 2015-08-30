
using System;
using System.Threading;

namespace ETrikeV
{
	//直角駐車
	public class RightAngleParkScenario : Scenario
	{
		private const int LIGHT_WIDTH = 10;
		private int endTachoCount;
		//private int speed;
		//private Mode edge;

		public RightAngleParkScenario (int endTachoCount, int speed, Mode edge)
		{
			this.endTachoCount = endTachoCount;
			//this.speed = speed;
			//this.edge = edge;
		}

		private void actionRightTurn(Ev3System sys, sbyte rightPw, sbyte leftPw, int slop, int distance)
		{
			int[] tmpDistance = new int[2];

			sys.stopMotors ();

			sys.setSteerSlope (slop);

			tmpDistance[0] = sys.leftMotorGetMoveCm();
			sys.setRightMotorPower(rightPw);
			sys.setLeftMotorPower(leftPw);

			if (distance > 0) {
				while (true) {
					tmpDistance [1] = sys.leftMotorGetMoveCm ();
					if (tmpDistance [1] > (tmpDistance [0] + distance)) {
						break;
					}
					//8ミリ秒待ち
					Thread.Sleep (8);
				}
			} else {
				while (true) {
					tmpDistance [1] = sys.leftMotorGetMoveCm ();
					if (tmpDistance [1] < (tmpDistance [0] + distance)) {
						break;
					}
					//8ミリ秒待ち
					Thread.Sleep (8);
				}
			
			}

			sys.stopMotors ();
		}

		private void actionLeftTurn(Ev3System sys, sbyte rightPw, sbyte leftPw, int slop, uint distance)
		{
			int[] tmpDistance = new int[2];

			sys.stopMotors ();

			sys.setSteerSlope (slop);

			tmpDistance[0] = sys.rightMotorGetMoveCm();
			sys.setRightMotorPower(rightPw);
			sys.setLeftMotorPower(leftPw);

			while (true)
			{
				tmpDistance[1] = sys.rightMotorGetMoveCm();
				if (tmpDistance[1] > (tmpDistance[0] + distance))
				{
					break;
				}
				//8ミリ秒待ち
				Thread.Sleep(8);
			}

			sys.stopMotors ();
		}

		public override bool run(Ev3System sys)
		{
			// 終了確認
			if (sys.getAverageTachoCount() > endTachoCount) {
				return true;
			}

			//スタート後に直進
			//actionStraight (sys, 100, 90);

			//右に-90度回転
			actionRightTurn(sys, 0, -50, 80, -30);		//30度より高い角度だと進みすぎて精密な制御ができない

			//ステアリングの傾きを正面に修正する
			//sys.setSteerSlope (0);

			//バックする
			//actionStraight (sys, -10, 100);

			//3秒停止(念のため4秒)
			Thread.Sleep(4000);

			//コース復帰
			//actionStraight (sys, 20, 100);
			actionRightTurn(sys, 50, 0, 60, 50);		//30度より高い角度だと進みすぎて精密な制御ができない

			//右に旋回
			//actionRightTurn(sys, -50, 50, 35, 11);		//30度より高い角度だと進みすぎて精密な制御ができない

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);

			return true;
		}
	}
}

