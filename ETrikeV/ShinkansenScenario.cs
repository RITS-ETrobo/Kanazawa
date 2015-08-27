using System;
using System.Threading;
using MonoBrickFirmware.Sensors;

namespace ETrikeV
{
	public class ShinkansenScenario : Scenario
	{
		private const int LIGHT_WIDTH = 10;
		private const int MAX_STEERING_ANGLE = 180;
		private const int STEER_POWER = 100;

		//フィギュアLシナリオに同一の関数あり、後でどうにかする
		private void actionStraight(Ev3System sys, int distance, int pw)
		{
			int nowDistance;
			int targetDistance;

			sys.stopMotors ();

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

			sys.stopMotors ();
		}

		//フィギュアLシナリオに同一の関数あり、後でどうにかする
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

		public override bool run(Ev3System sys)
		{
			int sonicDistance;
			int soicRange = 100;

			//同じ動作を2回するだけと気づいた
			for (int i = 0; i < 2; i++) {
				//新幹線を検知するまで停止
				while (true) {
					sonicDistance = sys.getsonic ();
					if (sonicDistance > soicRange) {
						break;
					}
				}
					
				//前進
				actionStraight (sys, 20 + (sonicDistance / 10), 80);

				//ライン復帰する
				//板上のラインに復帰するために首を振ってライン探索する
				serchLine (sys, 5);

				//ライントレースで前進（規定距離だけ）
				int nowDistance = (sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2;
				while (true) {
					lineTrace (sys, 50, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
					if ((sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2 > (nowDistance + 10)) { // 7
						break;
					}
					Thread.Sleep (5);
				}
			}

			return true;

		}
	}
}

