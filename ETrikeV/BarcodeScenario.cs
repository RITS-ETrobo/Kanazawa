﻿using System;
using System.Threading;
using MonoBrickFirmware.Sensors;

namespace ETrikeV
{
	public class BarcodeScenario : Scenario
	{
		/// <summary>
		/// 読み取ったバーコード情報
		/// </summary>
		int BarcodeBit = 0;


		private const int LIGHT_WIDTH = 10;
		private const int MAX_STEERING_ANGLE = 180;
		private const int STEER_POWER = 100;
		private const int RATIO = 25;

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
				lineTrace(sys, 30, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);
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

			//茶色を検知するまで
			while (sys.colorRead() != (int)Color.Brown)
			{
				Thread.Sleep(5);

				// 値は仮
				lineTrace(sys, 30, Mode.Left, LIGHT_WIDTH, MAX_STEERING_ANGLE/18, STEER_POWER/5);
				if (isStep(sys) == true)
				{
					break;
				}
			}
			//後処理
			sys.color.Mode = ColorMode.Reflection;
			Thread.Sleep(10);
		}

		/// <summary>
		/// バーコード読み取り
		/// </summary>
		/// <param name="sys"></param>
		private void redBarcode(Ev3System sys)
		{
			sys.color.Mode = ColorMode.Color;

			//値削除
			BarcodeBit = 0;

			//3cm単位でバーコード読み取り(3cm×10回)
			for (int loopCounter = 0; loopCounter <= 10; loopCounter++)
			{
				//10cm?
				actionStraight(sys, 10, 40);

				//白判定
				if (sys.colorRead() == (int)Color.White)
				{
					//白だったら1対象箇所にビットを立てる
					BarcodeBit += (int)(1 << loopCounter);
				}
				else
				{
					//何もしない
				}
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
			serchLine(sys, 5, true);

			//ステアリングの傾きを正面に修正する
			sys.setSteerSlope(0);

			//茶色を検知する
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

