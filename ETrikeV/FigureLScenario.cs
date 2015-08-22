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

			if (motorStopCount == 3)
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

			stopMotor (sys);

			nowDistance = ((sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2);
			targetDistance = nowDistance + distance;

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

		static void actionAdvanceToBlackLine(Ev3System sys, sbyte pw)
		{
			stopMotor (sys);

			sys.setSteerSlope (0);
			sys.leftMotor.SetPower((sbyte)(pw * -1));
			sys.rightMotor.SetPower((sbyte)(pw * -1));
			while (true)
			{
				if (sys.color.Read() < 25)
				{
					break;
				}
				//8ミリ秒待ち
				Thread.Sleep(1);
			}

			stopMotor (sys);

		}

		void serchLine(Ev3System sys)
		{
			int[] distance = new int[2];
			int serchLeftVal = 3;
			int serchrightVal = 6;
			int pw = 20;

			stopMotor (sys);

			sys.color.Mode = ColorMode.Color;
			Thread.Sleep(10);

			if (sys.colorRead () == (int)Color.Black) {
				//後処理　分散しているのでまとめたい
				sys.color.Mode = ColorMode.Reflection;
				Thread.Sleep(10);
				return;
			}
				
			sys.setSteerSlope(60);
			distance[0] = sys.leftMotorGetMoveCm ();
			sys.setLeftMotorPower(pw);
			sys.setRightMotorPower(pw * -1);

			//右回転
			while (true)
			{
				if (sys.colorRead () == (int)Color.Black) {
					//後処理　分散しているのでまとめたい
					stopMotor (sys);
					sys.color.Mode = ColorMode.Reflection;
					Thread.Sleep(10);
					return;
				}

				distance[1] = sys.leftMotorGetMoveCm ();
				if (distance[1] > (distance[0] + serchLeftVal))
				{
					break;
				}
				Thread.Sleep(1);
			}

			stopMotor (sys);

			sys.setSteerSlope(-60);
			distance [0] = sys.rightMotorGetMoveCm ();
			sys.setLeftMotorPower(pw * -1);
			sys.setRightMotorPower(pw);

			//左回転
			while (true)
			{
				if (sys.colorRead () == (int)Color.Black) {
					//後処理　分散しているのでまとめたい
					stopMotor (sys);
					sys.color.Mode = ColorMode.Reflection;
					Thread.Sleep(10);
					return;
				}

				distance [1] = sys.rightMotorGetMoveCm ();
				if (distance[1] > (distance[0] + serchrightVal))
				{
					break;
				}
				Thread.Sleep(1);
			}

			//後処理　分散しているのでまとめたい
			stopMotor (sys);
			sys.color.Mode = ColorMode.Reflection;
			Thread.Sleep(10);
			return;

		}

		static void actionTurn(Ev3System sys, sbyte rightPw, sbyte leftPw, int slop, uint distance)
		{
			int[] tacho = new int[2];
			int distanceToTacho = (int)(distance * 25); //距離をタコ回転数に変換した値。　２５回転で1cm

			stopMotor (sys);

			sys.setSteerSlope (slop);

			tacho[0] = sys.leftMotorGetTachoCount();
			sys.leftMotor.SetPower(leftPw);
			sys.rightMotor.SetPower(rightPw);

			while (true)
			{
				tacho[1] = sys.leftMotorGetTachoCount();
				if (tacho[1] < (tacho[0] - distanceToTacho))
				{
					break;
				}
				//8ミリ秒待ち
				Thread.Sleep(1);
			}

			stopMotor (sys);
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
			actionStraight(sys, 21, 70);

			//板上のラインに復帰するために首を振ってライン探索する
			serchLine (sys);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);

			//ライントレースで前進（規定距離だけ）
			int nowDistance = (sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2;
			while (true) {
				lineTrace(sys, 50, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if ((sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2 > (nowDistance + 15)) { // 7
					break;
				}
				Thread.Sleep (5);
			}
				
			//右に旋回
			//actionTurn(sys, 0, -40, 60, 5);

			//前進（ラインを見つけるまで。距離でもよいかも）
			//actionAdvanceToBlackLine (sys, 30);

			//弧を描くように前進（規定距離だけ）


			//ラインを探索して、終了

			stopMotor (sys);

			return true;
		}
	
	}
}

