﻿

using System;
using System.Threading;

namespace ETrikeV
{
	//直角駐車
	public class JuretsuParkScenario : Scenario
	{
		private const int LIGHT_WIDTH = 10;

		public JuretsuParkScenario ()
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

		public override bool run(Ev3System sys)
		{
			int cur, sta;
			sta = sys.leftMotorGetMoveCm ();

			sys.setSteerSlope (80);
			sys.rightMotorBrake ();
			sys.setLeftMotorPower (-70);
			while (true) {
				cur = sys.leftMotorGetMoveCm ();
				if (cur <= sta - 40) {
					sys.stopMotors ();
					break;
				}
			}

			return true;
		}
	}
}

