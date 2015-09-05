
using System;
using System.Threading;

namespace ETrikeV
{
	//直角駐車
	public class RightAngleParkScenario : Scenario
	{
		private const int LIGHT_WIDTH = 10;
		//private int speed;
		//private Mode edge;

		public RightAngleParkScenario ()
		{

		}

		private int getAbsParam(int param)
		{
			if (param < 0) {
				return param * -1;
			} else {
				return param;
			}
		}

		private void actionRightTurnBack(Ev3System sys, sbyte rightPw, sbyte leftPw, int distance)
		{
			int[] tmpDistance = new int[2];
			int slope = SteerCtrl.getSteeringAngle (getAbsParam(leftPw), getAbsParam(rightPw));;

			sys.stopMotors ();

			sys.setSteerSlope (slope);

			tmpDistance[0] = sys.leftMotorGetMoveCm();

			if (rightPw == 0) {
				sys.rightMotorBrake ();
			} else {
				sys.setRightMotorPower(rightPw * -1);
			}
			if (leftPw == 0) {
				sys.leftMotorBrake ();
			} else {
				sys.setLeftMotorPower (leftPw * -1);
			}

			while (true) {
				tmpDistance [1] = sys.leftMotorGetMoveCm ();
				if (tmpDistance [1] < (tmpDistance [0] - distance)) {
					break;
				}
				//8ミリ秒待ち
				Thread.Sleep (5);
			}

			sys.stopMotors ();
		}

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
				Thread.Sleep(5);
			}

			sys.stopMotors ();
		}

		public override bool run(Ev3System sys)
		{

			//直角駐車入庫
			/*******************************************/
			//右に-90度回転
			actionRightTurnBack(sys, 20, 85, 21);	//70 20
			Thread.Sleep(100);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);

			//バックする
			actionStraight (sys, -15, 40);
			Thread.Sleep(100);
			/*******************************************/

			//3秒停止
			Thread.Sleep(3500);

			//直角駐車出庫
			/*******************************************/
			actionStraight (sys, 17, 40);
			Thread.Sleep(100);
			actionRightTurn (sys, 10, 80, 17); //20
			Thread.Sleep(100);
			serchLine (sys, 4, false, Mode.Right);
			sys.setSteerSlope (0);

			/*******************************************/

			return true;
		}
	}
}

