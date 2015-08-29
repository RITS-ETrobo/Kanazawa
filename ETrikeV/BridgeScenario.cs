using System;
using System.Threading;
using MonoBrickFirmware;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;

namespace ETrikeV
{
	/// <summary>
	/// 二本橋
	/// </summary>
	public class BridgeScenario : Scenario
	{
		//段差検知用
		private static int[] steerCount = new int[2];
		private static int[] leftCount = new int[2];
		private static int[] rightCount = new int[2];
		private static uint motorStopCount = 0;

		private const int LIGHT_WIDTH = 10;
		private const int MAX_STEERING_ANGLE = 180;
		private const int STEER_POWER = 100;

		public BridgeScenario ()
		{

		}

		/// <summary>
		/// 二本橋の初期コード
		/// </summary>
		/// <param name="sys"></param>
		/// <returns></returns>
		public override bool run(Ev3System sys)
		{
			bool ret = false;

			//段差を検知するまでライントレースする
			while (true)
			{
				lineTrace(sys, 30, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if (isStep(sys) == true)
				{
					break;
				}
				Thread.Sleep(5);
			}

			//ステアリングの傾きを直す
			actionSlopeChange(sys.leftMotor, sys.rightMotor, sys.steerMotor, 0);

			//段差を超えるための助走のための後退
			actionBackward(sys.leftMotor, sys.rightMotor, sys.steerMotor, 100, 7, false);

			//ステアリングの傾きを直す
			actionSlopeChange(sys.leftMotor, sys.rightMotor, sys.steerMotor, 0);
			//前進して段差を超える
			actionAdvance(sys.leftMotor, sys.rightMotor, sys.steerMotor, 100, 40, false);

			//ステアリングの傾きを直す
			actionSlopeChange(sys.leftMotor, sys.rightMotor, sys.steerMotor, 0);

			//ライン復帰のためのライントレース
			int nowDistance = sys.getAverageMoveCM();

			while (true)
			{
				lineTrace(sys, 50, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if (sys.getAverageMoveCM() > (nowDistance + 8))
				{ // 7
					break;
				}
				Thread.Sleep(5);
			}

			return ret;
		}

		/// <summary>
		/// モーターブレーキ
		/// </summary>
		/// <param name="leftMotor">左モーター</param>
		/// <param name="rightMotor">右モーター</param>
		/// <param name="steerMotor">ステアリングモーター</param>
		static void stopMotor(Motor leftMotor, Motor rightMotor, Motor steerMotor)
		{
			leftMotor.Brake();
			rightMotor.Brake();
			steerMotor.Brake();
		}

		/// <summary>
		/// ステアリング傾き変更
		/// </summary>
		/// <param name="leftMotor">左モーター</param>
		/// <param name="rightMotor">右モーター</param>
		/// <param name="steerMotor">ステアリングモーター</param>
		/// <param name="slope">滑らか</param>
		static void actionSlopeChange(Motor leftMotor, Motor rightMotor, Motor steerMotor, int slope)
		{
			int tacho;
			int maxSlopeRange = 5;
			int slopeToTacho = slope * 8;

			//停止
			stopMotor(leftMotor, rightMotor, steerMotor);

			tacho = steerMotor.GetTachoCount();

			//前輪を真っ直ぐに治す
			while (true)
			{
				tacho = steerMotor.GetTachoCount();
				if ((tacho <= (slopeToTacho + maxSlopeRange)) && (tacho >= (slopeToTacho - maxSlopeRange)))
				{
					break;
				}

				if (tacho > (slopeToTacho + maxSlopeRange))
				{
					steerMotor.SetPower(-100);
				}
				else
				{
					steerMotor.SetPower(100);
				}

				Thread.Sleep(10);
			}

			//停止
			stopMotor(leftMotor, rightMotor, steerMotor);

		}

		/// <summary>
		/// タコメータの回転数を元に任意の距離だけ前進する
		/// </summary>
		/// <param name="leftMotor">左モーター</param>
		/// <param name="rightMotor">右モーター</param>
		/// <param name="steerMotor">ステアリングモーター</param>
		/// <param name="pw">モーターパワー</param>
		/// <param name="distance">移動距離(cm)</param>
		/// <param name="isSlip">滑らか</param>
		static void actionAdvance(Motor leftMotor, Motor rightMotor, Motor steerMotor, sbyte pw, uint distance, bool isSlip)
		{
			int[] tacho = new int[2];
			int distanceToTacho = (int)(distance * 25);	//距離をタコ回転数に変換した値。　２５回転で1cm
			sbyte maxPw = (sbyte)(pw * -1);
			sbyte cPw = 0;

			tacho[0] = leftMotor.GetTachoCount();	//前進するまえのタコメータ値を退避しておく

			//停止
			stopMotor(leftMotor, rightMotor, steerMotor);

			//滑らか駆動OFF
			if (isSlip == false)
			{
				cPw = maxPw;
				leftMotor.SetPower(cPw);
				rightMotor.SetPower(cPw);
			}

			while (true)
			{
				//滑らか駆動
				if ((isSlip == true) && (cPw != maxPw))
				{
					cPw--;
					leftMotor.SetPower(cPw);
					rightMotor.SetPower(cPw);
				}

				//現在のタコメーター値を取得し、指定した距離を超えたら抜ける
				tacho[1] = leftMotor.GetTachoCount();
				if (tacho[1] < (tacho[0] - distanceToTacho))
				{
					break;
				}
				Thread.Sleep(10);
			}

			//停止
			stopMotor(leftMotor, rightMotor, steerMotor);

		}

		/// <summary>
		/// タコメータの回転数を元に任意の距離だけ後退する
		/// </summary>
		/// <param name="leftMotor">左モーター</param>
		/// <param name="rightMotor">右モーター</param>
		/// <param name="steerMotor">ステアリングモーター</param>
		/// <param name="pw">モーターパワー</param>
		/// <param name="distance">移動距離(cm)</param>
		/// <param name="isSlip">滑らか</param>
		static void actionBackward(Motor leftMotor, Motor rightMotor, Motor steerMotor, sbyte pw, uint distance, bool isSlip)
		{
			int[] tacho = new int[2];
			int distanceToTacho = (int)(distance * 25);	//距離をタコ回転数に変換した値。　２５回転で1cm
			sbyte maxPw = pw;
			sbyte cPw = 0;

			tacho[0] = leftMotor.GetTachoCount();	//前進するまえのタコメータ値を退避しておく

			//停止
			stopMotor(leftMotor, rightMotor, steerMotor);

			//滑らか駆動OFF
			if (isSlip == false)
			{
				cPw = maxPw;
				leftMotor.SetPower(cPw);
				rightMotor.SetPower(cPw);
			}

			while (true)
			{
				//滑らか駆動
				if ((isSlip == true) && (cPw != maxPw))
				{
					cPw++;
					leftMotor.SetPower(cPw);
					rightMotor.SetPower(cPw);
				}

				//現在のタコメーター値を取得し、指定した距離を超えたら抜ける
				tacho[1] = leftMotor.GetTachoCount();
				if (tacho[1] > (tacho[0] + distanceToTacho))
				{
					break;
				}
				Thread.Sleep(10);
			}

			//停止
			stopMotor(leftMotor, rightMotor, steerMotor);

		}

		/// <summary>
		/// 段差検知
		/// 仕組みは後輪モーターのタコメータが変化しない＝段差に引っかかっている
		/// </summary>
		/// <param name="leftMotor">左モーター</param>
		/// <param name="rightMotor">右モーター</param>
		/// <param name="steerMotor">ステアリングモーター</param>
		/// <returns></returns>
		static bool isStep(Motor leftMotor, Motor rightMotor, Motor steerMotor)
		{
			leftCount[0] = leftMotor.GetTachoCount();
			rightCount[0] = rightMotor.GetTachoCount();
			steerCount[0] = steerMotor.GetTachoCount();

			if ((leftCount[0] == leftCount[1]) &&
				(rightCount[0] == rightCount[1]))
			{
				motorStopCount++;
			}
			else
			{
				steerCount[1] = steerCount[0];
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

	}
}