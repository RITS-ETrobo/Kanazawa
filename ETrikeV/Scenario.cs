using System;
using MonoBrickFirmware.Movement;
using System.Threading;
using MonoBrickFirmware.Sensors;

namespace ETrikeV
{
	public abstract class Scenario
	{
		private int[] leftCount = new int[2];
		private int[] rightCount = new int[2];
		private uint motorStopCount = 0;
		private double kp = 8.0;
		private double ki = 0.0;
		private double kd = 4.0;
		private int[] diff = new int[2];

		private const int serchMaxCount = 5;

		public abstract bool run (Ev3System robokon);

		// 基本的な走行メソッドはここに定義する //

		/// <summary>
		/// ライントレース
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="edge">Edge.</param>
		/// <param name="margin">Margin.</param>
		/// <param name="maxAngle">Max angle.</param>
		/// <param name="steerPwr">Steer pwr.</param>
		protected void lineTrace(Ev3System sys, int speed, Mode edge, int margin, int maxAngle, int steerPwr)
		{
			int light = sys.colorRead ();
			int steerCnt = sys.steerGetTachoCount ();
			if (edge == Mode.Left) {
				if (light > sys.TargetLight + margin) {
					// 白い場合は右に曲がる
					if (steerCnt < maxAngle) {
						sys.setSteerPower (steerPwr);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (0);
				} else if (light < sys.TargetLight - margin) {
					// 黒い場合は左に曲がる
					if (steerCnt > -1 * maxAngle) {
						sys.setSteerPower (-1 * steerPwr);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (0);
					sys.setRightMotorPower (speed);
				} else {
					// 灰色から規定範囲内ならステアリングをとめてゆっくり進む
					sys.steerBrake ();
					sys.setLeftMotorPower (speed / 2);
					sys.setRightMotorPower (speed / 2);
				}
			} else {
				if (light > sys.TargetLight + margin) {
					// 白い場合は左に曲がる
					if (steerCnt > -1 * maxAngle) {
						sys.setSteerPower (-1 * steerPwr);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (0);
					sys.setRightMotorPower (speed);
				} else if (light < sys.TargetLight - margin) {
					// 黒い場合は右に曲がる
					if (steerCnt < maxAngle) {
						sys.setSteerPower (steerPwr);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (0);
				} else {
					// 灰色から規定範囲内ならステアリングをとめてゆっくり進む
					sys.steerBrake ();
					sys.setLeftMotorPower (speed / 2);
					sys.setRightMotorPower (speed / 2);
				}
			}
		}

		/// <summary>
		/// PID制御のライントレース
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="edge">Edge.</param>
		protected void lineTrace2(Ev3System sys, int speed, Mode edge)
		{
			int leftP, rightP;
			int light = sys.colorRead ();
			int pidVal = pid(sys, light);

			if (edge == Mode.Right) {
				pidVal *= -1;
			}

			leftP = speed;
			rightP = speed;
			if (pidVal < 0) {
				leftP -= leftP * pidVal * -1 / 100;
			} else if (pidVal > 0) {
				rightP -= rightP * pidVal / 100;
			}

			steerCtrl (sys, leftP, rightP);
			sys.setLeftMotorPower (leftP);
			sys.setRightMotorPower (rightP);
		}

		/// <summary>
		/// ストレート
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="edge">Edge.</param>
		/// <param name="margin">Margin.</param>
		protected void straightTrace(Ev3System sys, int speed, Mode edge, int margin)
		{
			// 後輪を制御してラインを走行する
			// ステアリングは一切変えない
			int leftMotorPower = speed;
			int rightMotorPower = speed;
			int light = sys.colorRead ();
			if (edge == Mode.Left) {
				if (light > sys.TargetLight + margin) {
					rightMotorPower /= 2;
				} else if (light < sys.TargetLight - margin) {
					leftMotorPower /= 2;
				}
			} else {
				if (light > sys.TargetLight + margin) {
					leftMotorPower /= 3;
				} else if (light < sys.TargetLight - margin) {
					rightMotorPower /= 2;
				}
			}

			sys.setLeftMotorPower (leftMotorPower);
			sys.setRightMotorPower (rightMotorPower);
		}

		/// <summary>
		/// 指定した距離だけ走行する（ライントレースはしない）
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="distance">Distance.</param>
		/// <param name="pw">Pw.</param>
		protected void actionStraight(Ev3System sys, int distance, int pw)
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

		private bool serchRight(Ev3System sys, int pw, int range, Mode edge)
		{
			int[] distance = new int[2];
			int leftPw = pw;
			int rightPw = (pw * -1);
			int slope = 90;
			bool blackFound = false;

			/***********************************************/
			sys.setSteerSlope(slope); //60
			distance[0] = sys.leftMotorGetMoveCm ();
			sys.setLeftMotorPower(leftPw);
			sys.setRightMotorPower(rightPw);

			//右回転
			while (true)
			{
				if (sys.colorRead () == (int)Color.Black) {
					if (edge == Mode.Right) {
						blackFound = true;
					} else {
						sys.stopMotors ();
						return true;
					}
				} else if (blackFound && sys.colorRead () == (int)Color.White) {
					sys.stopMotors ();
					return true;
				}

				distance[1] = sys.leftMotorGetMoveCm ();
				if (distance[1] > (distance[0] + range))
				{
					break;
				}
				Thread.Sleep(5); //1
			}

			sys.stopMotors ();
			/***********************************************/

			return false;
		}

		private bool serchLeft(Ev3System sys, int pw, int range, Mode edge)
		{
			int[] distance = new int[2];
			int leftPw = (pw * -1);
			int rightPw = pw;
			int slope = -90;
			bool blackFound = false;

			/***********************************************/
			sys.setSteerSlope(slope); //70
			distance [0] = sys.rightMotorGetMoveCm ();
			sys.setLeftMotorPower(leftPw);
			sys.setRightMotorPower(rightPw);

			//左回転
			while (true)
			{
				if (sys.colorRead () == (int)Color.Black) {
					if (edge == Mode.Left) {
						blackFound = true;
					} else {
						sys.stopMotors ();
						return true;
					}
				} else if (blackFound && sys.colorRead () == (int)Color.White) {
					sys.stopMotors ();
					return true;
				}

				distance [1] = sys.rightMotorGetMoveCm ();
				if (distance[1] > (distance[0] + range))
				{
					break;
				}
				Thread.Sleep(5); //1
			}

			sys.stopMotors ();
			/***********************************************/

			return false;
		}

		/// <summary>
		/// ラインを探す
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="range">Range.</param>
		protected  void serchLine(Ev3System sys, int range, bool isStartRight, Mode edge)
		{
			int pw = 20; //30

			sys.stopMotors ();
			sys.color.Mode = ColorMode.Color;
			Thread.Sleep(10);

			if (sys.colorRead () == (int)Color.Black) {
				//後処理　分散しているのでまとめたい
				sys.color.Mode = ColorMode.Reflection;
				Thread.Sleep(10);

				return;
			}
				
			if (isStartRight == true) {
				if (serchRight(sys, pw, range, edge) != true) {

					serchLeft(sys, pw, range * 2, edge);

				} else {
					//処理不要
				}
			} else {
				if (serchLeft(sys, pw, range, edge) != true) {

					serchRight(sys, pw, range * 2, edge);

				} else {
					//処理不要
				}
			}

			//後処理
			sys.color.Mode = ColorMode.Reflection;
			Thread.Sleep(10);

			return;
		}

		/// <summary>
		/// 段差を検知する
		/// </summary>
		/// <returns><c>true</c>, 段差を検知した, <c>false</c> 段差を検知していない.</returns>
		/// <param name="sys">Sys.</param>
		protected  bool isStep(Ev3System sys)
		{
			const int stopCount = 5;

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
			if (motorStopCount == stopCount)
			{
				motorStopCount = 0;
				return true;
			}

			return false;
		}

		/// <summary>
		/// PID制御（Iなし）
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="light">Light.</param>
		private int pid(Ev3System sys, int light)
		{
			int ret = 0;
			double p, d;

			diff [1] = diff [0];
			diff [0] = light - sys.TargetLight;

			p = kp * diff [0];
			d = kd * (diff [0] - diff [1]);

			ret = (int)(p + d);
			ret = (ret > 100) ? 100 : ret;
			ret = (ret < -100) ? -100 : ret;

			return ret;
		}

		/// <summary>
		/// ステアリング制御
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="leftPwr">Left pwr.</param>
		/// <param name="rightPwr">Right pwr.</param>
		private void steerCtrl(Ev3System sys, int leftPwr, int rightPwr)
		{
			int target = SteerCtrl.getSteeringAngle (leftPwr, rightPwr);
			target *= 8; // degree -> tacho
			int current = sys.steerGetTachoCount ();
			int diff = (target - current) * 2;
			diff = (diff > 100) ? 100 : diff;
			diff = (diff < -100) ? -100 : diff;
			sys.setSteerPower (diff);

			return;
		}

	}
}

