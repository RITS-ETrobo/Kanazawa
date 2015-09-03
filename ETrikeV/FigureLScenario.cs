using System;
using System.Threading;

namespace ETrikeV
{

	public class FigureLScenario : Scenario
	{

		private const int LIGHT_WIDTH = 5;//10
		private const int MAX_STEERING_ANGLE = 160; //180
		private const int STEER_POWER = 100;

		/// <summary>
		/// 右に曲がる
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="rightPw">Right pw.</param>
		/// <param name="leftPw">Left pw.</param>
		/// <param name="slop">Slop.</param>
		/// <param name="distance">Distance.</param>
		private void actionRightTurn(Ev3System sys, sbyte rightPw, sbyte leftPw, uint distance)
		{
			int[] tmpDistance = new int[2];
			int slope = SteerCtrl.getSteeringAngle (leftPw, rightPw);

			sys.stopMotors ();

			sys.setSteerSlope (slope);

			tmpDistance[0] = sys.leftMotorGetMoveCm();
			if (rightPw == 0) {
				sys.rightMotorBrake ();
			} else {
				sys.setRightMotorPower (rightPw);
			}
			if (leftPw == 0) {
				sys.leftMotorBrake ();
			} else {
				sys.setLeftMotorPower (leftPw);
			}

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

			//段差を検知する
			/*************************************************/
			while (true) {
				//lineTrace(sys, 30, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				straightTrace (sys, 20, Mode.Left, LIGHT_WIDTH);
				if (isStep (sys) == true) {
					sys.stopMotors ();
					break;
				}
				Thread.Sleep (5);
			}
			/*************************************************/

			//板を登る
			/*************************************************/
			//ステアリングの傾きを正面に修正する
			actionStraight(sys, -1, 30);
			sys.setSteerSlope (0);

			//段差を超えるために高いスピードで前進する
			actionStraight(sys, 5, 40); 
			Thread.Sleep(10);
			actionStraight(sys, 9, 80); 	//10
			/*************************************************/

			//板上のラインに復帰する
			/*************************************************/
			//板上のラインに復帰するために首を振ってライン探索する
			serchLine (sys, 2, true);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);
			/*************************************************/

			//右旋回位置までライン上を移動する
			/*************************************************/
			//ライントレースで前進（規定距離だけ）
			int nowDistance = sys.getAverageMoveCM();
			while (true) {
				lineTrace(sys, 50, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				//straightTrace (sys, 30, Mode.Left, LIGHT_WIDTH);
				if (sys.getAverageMoveCM() > (nowDistance + 18)) { // 16
					sys.stopMotors ();
					Thread.Sleep(100);
					break;
				}
				Thread.Sleep (5);
			}
			/*************************************************/

			//右に旋回する
			/*************************************************/
			//actionRightTurn(sys, 10, 50, 30, 12);	//11
			//actionRightTurn(sys, 15, 50, 30, 17);	//これはかなり良い
			actionRightTurn(sys, 16, 50, 19);	//
			Thread.Sleep(100);

			/*************************************************/

			//板から下りる
			/*************************************************/
			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);

			//板から降りる
			//斜め方向に進むので、完了後はライン探索が必須
			actionStraight(sys, 10, 40); //10
			/*************************************************/

			//ライン上に復帰する
			/*************************************************/
			//板から下りてラインを探す
			serchLine (sys, 2, true); //5

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);
			/*************************************************/

			//新幹線手前までライン上を進む
			/*************************************************/
			//ライン復帰のためのライントレース
			nowDistance = sys.getAverageMoveCM();
			while (true) {
				lineTrace(sys, 50, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if (sys.getAverageMoveCM() > (nowDistance + 8)) { // 7
					sys.stopMotors ();
					break;
				}
				Thread.Sleep (5);
			}
			/*************************************************/

			return true;
		}
	
	}
}

