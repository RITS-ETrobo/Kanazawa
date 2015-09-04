using System;
using System.Threading;
using MonoBrickFirmware.Sensors;

namespace ETrikeV
{
	/// <summary>
	/// バーコードの出口から二本橋手前のストレートに逃げるシナリオ
	/// </summary>
	public class ShortcutScenario : Scenario
	{
		public ShortcutScenario ()
		{
		}

		public override bool run (Ev3System sys)
		{
			int currentTacho;
			int startTacho = sys.rightMotorGetTachoCount ();

			// 左ターン
			sys.setSteerSlope (SteerCtrl.getSteeringAngle(0, 60));
			sys.leftMotorBrake();
			sys.setRightMotorPower (60);
			while (true) {
				currentTacho = sys.rightMotorGetTachoCount ();
				if (currentTacho >= startTacho + 280) {
					sys.rightMotorBrake ();
					break;
				}
			}
			sys.setSteerSlope (0);

			// 直進
			actionStraight(sys, 68, 80);
			Thread.Sleep (1000);

			// ラインまで直進
			bool blackFound = false;
			sys.color.Mode = ColorMode.Color;
			sys.setLeftMotorPower (80);
			sys.setRightMotorPower (80);
			while (true) {
				if (sys.colorRead () == (int)Color.Black) {
					blackFound = true;
				}
				if (blackFound && sys.colorRead () == (int)Color.White) {
					sys.stopMotors ();
					sys.color.Mode = ColorMode.Reflection;
					break;
				}
			}
			actionStraight(sys, 5, 50);
			Thread.Sleep (1000);

			// 右ターン
			startTacho = sys.leftMotorGetTachoCount();
			sys.setSteerSlope (SteerCtrl.getSteeringAngle(60, 0));
			sys.rightMotorBrake ();
			sys.setLeftMotorPower (60);
			while (true) {
				currentTacho = sys.leftMotorGetTachoCount ();
				if (currentTacho >= startTacho + 180) {
					sys.leftMotorBrake ();
					break;
				}
			}
			sys.setSteerSlope (0);

			// ライントレース
			startTacho = sys.getAverageTachoCount();
			while (true) {
				lineTrace (sys, 80, Mode.Left, 5, 240, 100);
				currentTacho = sys.getAverageTachoCount ();
				if (currentTacho >= startTacho + 500) {
					sys.stopMotors ();
					break;
				}
			}
			sys.setSteerSlope (0);

			return true;
		}
	}
}

