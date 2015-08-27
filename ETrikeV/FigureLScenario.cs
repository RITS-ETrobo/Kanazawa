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

		private int[] leftCount = new int[2];
		private int[] rightCount = new int[2];
		private uint motorStopCount = 0;
			
		/// <summary>
		/// 段差を検知する
		/// </summary>
		/// <returns><c>true</c>, 段差を検知した, <c>false</c> 段差を検知していない.</returns>
		/// <param name="sys">Sys.</param>
		private bool isStep(Ev3System sys)
		{
			// 現在の後輪のタコを取得する
			leftCount [0] = sys.leftMotorGetTachoCount ();
			rightCount [0] = sys.rightMotorGetTachoCount ();

			// 前回の後輪のタコと比較する
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

			// 3回回転しない状態が続いたら段差とする
			if (motorStopCount == 3)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// 指定した距離だけ走行する（ライントレースはしない）
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="distance">Distance.</param>
		/// <param name="pw">Pw.</param>
		private void actionStraight(Ev3System sys, int distance, int pw)
		{
			int nowDistance;
			int targetDistance;

			sys.stopMotors ();

			nowDistance = sys.getAverageMoveCM();
			targetDistance = nowDistance + distance;

			//前進と後進で距離判定の条件式が異なるので分ける
			if (distance > 0) {

				sys.setLeftMotorPower (pw);
				sys.setRightMotorPower (pw);

				while (true) {
					if (sys.getAverageMoveCM() > targetDistance) {
						break;
					}
					Thread.Sleep (10);
				}
			} else {

				sys.setLeftMotorPower (pw * -1);
				sys.setRightMotorPower (pw * -1);

				while (true) {
					if (sys.getAverageMoveCM() < targetDistance) {
						break;
					}
					Thread.Sleep (10);
				}
			}

			sys.stopMotors ();
		}

		/// <summary>
		/// 黒を検知するまで直進する
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="pw">Pw.</param>
		private void actionAdvanceToBlackLine(Ev3System sys, int pw)
		{
			sys.stopMotors ();

			sys.setSteerSlope (0);
			sys.setLeftMotorPower(pw);
			sys.setRightMotorPower(pw);

			//センサーモードが反射光モードであることが前提のため、チェック処理を入れてもよいかも
			while (true)
			{
				if (sys.color.Read() < 25)
				{
					break;
				}

				Thread.Sleep(1);
			}

			sys.stopMotors ();

		}

		/// <summary>
		/// ラインを探す
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="range">Range.</param>
		private void serchLine(Ev3System sys, int range)
		{
			int[] distance = new int[2];
			int serchLeftVal = range;
			int serchrightVal = (range * 2);
			int pw = 30;

			sys.stopMotors ();

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
					sys.stopMotors ();
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

			sys.stopMotors ();

			sys.setSteerSlope(-60);
			distance [0] = sys.rightMotorGetMoveCm ();
			sys.setLeftMotorPower(pw * -1);
			sys.setRightMotorPower(pw);

			//左回転
			while (true)
			{
				if (sys.colorRead () == (int)Color.Black) {
					//後処理　分散しているのでまとめたい
					sys.stopMotors ();
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
			sys.stopMotors ();
			sys.color.Mode = ColorMode.Reflection;
			Thread.Sleep(10);
			return;

		}

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
			serchLine (sys, 5);

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
			actionRightTurn(sys, -60, 60, 45, 7);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope (0);

			//板から降りる
			//斜め方向に進むので、完了後はライン探索が必須
			actionStraight(sys, 16, 100); //15

			//板から下りてラインを探す
			serchLine (sys, 15);

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

