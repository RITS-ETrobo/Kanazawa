using System;
using MonoBrickFirmware.Sensors;
using System.Threading;
using MonoBrickFirmware.Movement;

namespace ETrikeV
{

	public class FigureLScenario : Scenario
	{

		private const int LIGHT_WIDTH = 10;
		private const int MAX_STEERING_ANGLE = 180;
		private const int STEER_POWER = 100;

		static int[] leftCount = new int[2];
		static int[] rightCount = new int[2];
		static uint motorStopCount = 0;
			
		static bool isStep(Ev3System sys)
		{

			leftCount [0] = sys.leftMotorGetTachoCount ();
			rightCount [0] = sys.rightMotorGetTachoCount ();

			if ((leftCount[0] == leftCount[1]) && (rightCount[0] == rightCount[1]))
			{
				motorStopCount++;
			}
			else
			{
				leftCount[1] = leftCount[0];
				rightCount[1] = rightCount[0];
				motorStopCount = 0;
			}

			if (motorStopCount == 2)
			{
				return true;
			}

			return false;
		}

		static void stopMotor(Ev3System sys)
		{
			sys.leftMotorBrake ();
			sys.rightMotorBrake ();
			sys.steerBrake ();
		}

		static void actionStraight(Ev3System sys, int distance, int pw)
		{
			int nowDistance;
			int targetDistance;

			nowDistance = ((sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2);
			targetDistance = nowDistance + distance;

			stopMotor (sys);

			//前進と後進で距離判定の条件式が異なるので分ける
			if (distance > 0) {

				sys.setLeftMotorPower (pw);
				sys.setRightMotorPower (pw);

				while (true) {
					if ((sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2 > targetDistance) {
						break;
					}
					Thread.Sleep (10);
				}
			} else {

				sys.setLeftMotorPower (pw * -1);
				sys.setRightMotorPower (pw * -1);

				while (true) {
					if ((sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2 < targetDistance) {
						break;
					}
					Thread.Sleep (10);
				}
			}

			stopMotor (sys);
		}

		void serchLine(Ev3System sys)
		{
			int[] tacho = new int[2];
			int steerVal = 10;

			if (sys.colorRead () == (int)Color.Black) {
				return;
			}

			sys.setSteerSlope(-60);
			tacho[0] = sys.rightMotorGetTachoCount();
			sys.setLeftMotorPower(-20);
			sys.setRightMotorPower(20);

			//左回転
			while (true)
			{
				if (sys.colorRead () == (int)Color.Black) {
					return;
				}
					
				tacho[1] = sys.rightMotorGetTachoCount();
				if (tacho[1] < (tacho[0] - (25 * (steerVal / 2))))
				{
					break;
				}
				Thread.Sleep(1);
			}


			sys.setSteerSlope(60);
			tacho[0] = sys.leftMotorGetTachoCount();
			sys.setLeftMotorPower(20);
			sys.setRightMotorPower(-20);

			//左回転
			while (true)
			{
				if (sys.colorRead () == (int)Color.Black) {
					return;
				}

				tacho[1] = sys.leftMotorGetTachoCount();
				if (tacho[1] < (tacho[0] - (25 * steerVal)))
				{
					break;
				}
				Thread.Sleep(1);
			}

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
			actionStraight(sys, -4, 40);

			//段差を超えるために高いスピードで前進する
			actionStraight(sys, 20, 70);

			//板上のラインに復帰するために首を振ってライン探索する
			serchLine (sys);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);

			//ライントレースで前進（規定距離だけ）
			int nowDistance = (sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2;
			while (true) {
				lineTrace(sys, 30, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if ((sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2 > (nowDistance + 7)) {
					break;
				}
				Thread.Sleep (5);
			}


			//右に旋回


			//前進（ラインを見つけるまで。距離でもよいかも）


			//弧を描くように前進（規定距離だけ）


			//ラインを探索して、終了

			return true;
		}
	
	}
}

