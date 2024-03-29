﻿using System;

namespace ETrikeV
{
	public class SteerCtrl
	{
		/// <summary>
		/// 駆動輪間の距離(mm)
		/// </summary>
		public const int TREAD = 121;

		/// <summary>
		/// 前輪と駆動輪との距離(mm)
		/// </summary>
		public const int WHEEL_BASE = 212;

		private SteerCtrl ()
		{
		}

		/// <summary>
		/// 駆動輪のパワーから最適なステアリング角度を返します
		/// </summary>
		/// <returns>ステアリング角度(degree)</returns>
		/// <param name="leftMotorPower">Left motor power.</param>
		/// <param name="rightMotorPower">Right motor power.</param>
		public static int getSteeringAngle(int leftMotorPower, int rightMotorPower)
		{
			int target = 0;

			if (leftMotorPower != rightMotorPower) {
				double r = getRadius (leftMotorPower, rightMotorPower);
				target = (int)(Math.Atan (WHEEL_BASE / r) * 180 / Math.PI * -1);
			}

			return target;
		}

		/// <summary>
		/// カーブ半径を返します(mm)
		/// </summary>
		/// <returns>The radius.</returns>
		/// <param name="leftMotorPower">Left motor power.</param>
		/// <param name="rightMotorPower">Right motor power.</param>
		public static double getRadius(int leftMotorPower, int rightMotorPower)
		{
			double radius = 0.0;

			if (leftMotorPower != rightMotorPower) {
				radius = (rightMotorPower + leftMotorPower) / (rightMotorPower - leftMotorPower) * (TREAD / 2);
			}

			return radius;
		}
	}
}

