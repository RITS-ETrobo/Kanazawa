using System;
using System.Threading;
using MonoBrickFirmware.Sensors;

namespace ETrikeV
{
	/*
    バーコードはこのような仕様
    出口側に線の切れ目がある

     進行方向→
    ┌───────────┐
    │     ┌─────┐   │
    ├──-┤ | | | | |│  -┤
    │     └─────┘   │
    └───────────┘

    */

	public class BarcodeScenario : Scenario
	{
		/// <summary>
		/// 読み取ったバーコード情報
		/// </summary>
		int BarcodeBit = 0;

		private const int LIGHT_WIDTH = 10;
		private const int MAX_STEERING_ANGLE = 180;
		private const int STEER_POWER = 100;

		/// <summary>
		/// Initializes a new instance of the <see cref="ETrikeV.BarcodeScenario"/> class.
		/// </summary>
		public BarcodeScenario ()
		{
			// 何もなし
		}

		/// <summary>
		/// 段差検知
		/// </summary>
		/// <returns></returns>
		private void differenceCheck(Ev3System sys)
		{
			//段差を検知するまでライントレースする
			while (true)
			{
				lineTrace(sys, 30, Mode.Right, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
				if (isStep(sys) == true)
				{
					break;
				}
				Thread.Sleep(5);
			}

			return;

		}

		/// <summary>
		/// バーコードの開始位置検索
		/// </summary>
		/// <param name="sys"></param>
		private void findStartPosition(Ev3System sys)
		{
			sys.stopMotors();
			sys.color.Mode = ColorMode.Color;
			Thread.Sleep(5);

			//白色を検知するまで
			while (sys.colorRead() != (int)Color.White)
			{
				Thread.Sleep(5);

				// 値は仮
				// 黒or緑　とそれ以外ライントレースするようなコードを作成する。
				barcodeLineTrace(sys, 30, MAX_STEERING_ANGLE / 18, STEER_POWER / 5);
				if (isStep(sys) == true)
				{
					break;
				}
			}
			//後処理
			//sys.color.Mode = ColorMode.Reflection;
			//次の処理で使う前提なので、センサーモードは戻さない
			Thread.Sleep(10);
		}

		/// <summary>
		/// ライントレース
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="maxAngle">Max angle.</param>
		/// <param name="steerPwr">Steer pwr.</param>
		protected void barcodeLineTrace(Ev3System sys, int speed, int maxAngle, int steerPwr)
		{
			int light = sys.colorRead();
			int steerCnt = sys.steerGetTachoCount();
			//白・黄色だったら
			if (light == (int)Color.White || light == (int)Color.Yellow)
			{
				// 白い場合は右に曲がる
				if (steerCnt < maxAngle)
				{
					sys.setSteerPower(steerPwr);
				}
				else
				{
					sys.steerBrake();
				}
				sys.setLeftMotorPower(speed);
				sys.setRightMotorPower(0);
			}
			//黒・緑だったっら
			else if (light == (int)Color.Black || light == (int)Color.Green)
			{
				// 黒い場合は左に曲がる
				if (steerCnt > -1 * maxAngle)
				{
					sys.setSteerPower(-1 * steerPwr);
				}
				else
				{
					sys.steerBrake();
				}
				sys.setLeftMotorPower(0);
				sys.setRightMotorPower(speed);
			}
			else
			{
				// 規定した色以外範囲内ならステアリングをとめてゆっくり進む
				sys.steerBrake();
				sys.setLeftMotorPower(speed / 2);
				sys.setRightMotorPower(speed / 2);
			}
		}

		/// <summary>
		/// バーコード読み取り
		/// </summary>
		/// <param name="sys"></param>
		private void redBarcode(Ev3System sys)
		{
			//sys.color.Mode = ColorMode.Color;
			//カラーセンサーでくるので、処理は要らない

			//値削除
			BarcodeBit = 0;

			//3cm単位でバーコード読み取り(3cm×10回)
			for (int loopCounter = 0, color = 0; loopCounter <= 10; loopCounter++)
			{
				color = sys.colorRead();

				//白判定
				if (color == (int)Color.White)
				{
					//白だったら1対象箇所にビットを立てる
					BarcodeBit += (int)(1 << loopCounter);
				}
				else if(color == (int)Color.Brown)
				{
					//途中で茶色を見つけたらNG リカバリ処理は考えていない
					break;
				}

				//10cm?
				actionStraight(sys, 10, 40);
			}

			sys.color.Mode = ColorMode.Reflection;
			return;
		}

		/// <summary>
		/// バーコード
		/// </summary>
		/// <param name="sys">Sys.</param>
		public override bool run(Ev3System sys)
		{

			//段差検知
			differenceCheck(sys);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope(0);

			//段差を超えるための助走分だけ後退する
			actionStraight(sys, -5, 40);

			//段差を超えるために高いスピードで前進する
			actionStraight(sys, 22, 70);

			//板上のラインに復帰するために首を振ってライン探索する
			serchLine(sys, 5, true, Mode.Right);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope(0);

			//白色を検知する
			findStartPosition(sys);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope(0);

			//バーコード読み取り
			redBarcode(sys);

			return true;

		}

		/// <summary>
		/// バーコードの値を渡す
		/// </summary>
		/// <returns>The barcode bit.</returns>
		public int getBarcodeBit()
		{
			return BarcodeBit;
		}
	}
}

