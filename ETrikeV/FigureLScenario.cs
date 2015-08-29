using System;
using System.Threading;

namespace ETrikeV
{

	public class FigureLScenario : Scenario
	{

		private const int LIGHT_WIDTH = 10;
		private const int MAX_STEERING_ANGLE = 180;
		private const int STEER_POWER = 100;

		/// <summary>
		/// 右に曲がる
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="rightPw">Right pw.</param>
		/// <param name="leftPw">Left pw.</param>
		/// <param name="slop">Slop.</param>
		/// <param name="distance">Distance.</param>
		private void actionRightTurn(Ev3System sys, sbyte rightPw, sbyte leftPw, int slop, uint distance)
		{
			int[] tmpDistance = new int[2];

			sys.stopMotors ();

			sys.setSteerSlope (slop);

			tmpDistance[0] = sys.leftMotorGetMoveCm();
			sys.setRightMotorPower(rightPw);
			sys.setLeftMotorPower(leftPw);

			while (true)
			{
				tmpDistance[1] = sys.leftMotorGetMoveCm();
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

			//段差を検知するまでライントレースする
			while (true) {
				lineTrace(sys, 30, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if (isStep (sys) == true) {
					break;
				}
				Thread.Sleep (5);
			}

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);

			//段差を超えるための助走分だけ後退する
			actionStraight(sys, -5, 40);

			//段差を超えるために高いスピードで前進する
			actionStraight(sys, 22, 70);

			//板上のラインに復帰するために首を振ってライン探索する
			serchLine (sys, 5, true);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);

			//ライントレースで前進（規定距離だけ）
			int nowDistance = sys.getAverageMoveCM();
			while (true) {
				lineTrace(sys, 50, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if (sys.getAverageMoveCM() > (nowDistance + 16)) { // 7
					break;
				}
				Thread.Sleep (5);
			}
				
			//右に旋回
			actionRightTurn(sys, -50, 50, 30, 9);		//30度より高い角度だと進みすぎて精密な制御ができない

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);

			//板から降りる
			//斜め方向に進むので、完了後はライン探索が必須
			actionStraight(sys, 15, 100); //15

			//板から下りてラインを探す
			serchLine (sys, 15, true);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);

			//ライン復帰のためのライントレース
			nowDistance = sys.getAverageMoveCM();
			while (true) {
				lineTrace(sys, 50, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if (sys.getAverageMoveCM() > (nowDistance + 8)) { // 7
					break;
				}
				Thread.Sleep (5);
			}

			sys.stopMotors ();

			return true;
		}
	
	}
}

