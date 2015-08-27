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

		public override bool run(Ev3System sys)
		{
			int sonicDistance;
			int sonicMaxRange = 150; //80
			int sonicMinRage = 50;
			
			//新幹線を検知するまで停止
			sys.stopMotors ();
			sys.setSteerSlope (0);

			while (true) {
				sonicDistance = sys.getsonic ();
				if ((sonicDistance < sonicMaxRange) && (sonicDistance > sonicMinRage)) {
					Thread.Sleep (1000);	//1秒
					break;
				}
			}
				
			//前進
			actionStraight (sys, 13 + (sonicDistance / 10), 80);

			//ライン復帰する
			//板上のラインに復帰するために首を振ってライン探索する
			serchLine (sys, 2, false);
			sys.setSteerSlope (0);

			//ライントレースで前進（規定距離だけ）
			int nowDistance = (sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2;
			while (true) {
				lineTrace (sys, 50, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if ((sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2 > (nowDistance + 11)) { // 7
					break;
				}
				Thread.Sleep (5);
			}

			//突入時と同じ処理だが、パラメータが異なる

			//新幹線を検知するまで停止
			sys.stopMotors ();
			sys.setSteerSlope (0);

			while (true) {
				sonicDistance = sys.getsonic ();
				if ((sonicDistance < sonicMaxRange) && (sonicDistance > sonicMinRage)) {
					Thread.Sleep (1000);	//1秒
					break;
				}
			}

			//前進
			actionStraight (sys, 11 + (sonicDistance / 10), 80);

			//ライン復帰する
			//板上のラインに復帰するために首を振ってライン探索する
			serchLine (sys, 5, false);
			sys.setSteerSlope (0);

			//ライントレースで前進（規定距離だけ）
			nowDistance = (sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2;
			while (true) {
				lineTrace (sys, 50, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if ((sys.leftMotorGetMoveCm () + sys.rightMotorGetMoveCm ()) / 2 > (nowDistance + 10)) { // 7
					break;
				}
				Thread.Sleep (5);
			}

			return true;

		}
	}
}

