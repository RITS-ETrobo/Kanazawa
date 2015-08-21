using System;
using MonoBrickFirmware;
using MonoBrickFirmware.Display.Dialogs;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using System.Threading;
using MonoBrickFirmware.Sensors;

namespace ETrikeV
{
	class Program
	{
		//konさんの定義値
		const int MAX_STEERING_ANGLE = 180;
		const int DRIVING_POWER = 100;
		const int CONER_DRV_PWR = 70;
		const int LINE_TRACE_DRV_PWR = 70;
		const int STEER_POWER = 100;
		const int LIGHT_WIDTH = 10;

		//iidaさんの定義値
		//モータ系
		// const int MAX_STEERING_ANGLE = 500;
		const int STEERING_POWER = 30;
		const int OUT_WHEEL_POWER = 40;     //走行体的に前進
		const int IN_WHEEL_POWER = 1;       //走行体的に後退
		const int POWER_MAX = 100;

		//システム系
		static int sleepTime = 8;   //周期	単位はms
		//int gray = 25;              // グレイと判断する値(初期値)

		//PID系
		const double kp = 2.0;
		const double ki = 1.0;
		const double kd = 1.0;
		static int[] diff = new int[2];
		static double integral = 0.0;
		static bool isFirstPID = true;

		//段差検知用
		static int[] steerCount = new int[2];
		static int[] leftCount = new int[2];
		static int[] rightCount = new int[2];
		static uint motorStopCount = 0;

		enum stepKind : int { L_CURVE, TWO_BRIDGE, BARCODE };

		/// <summary>
		/// Main
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			Scenario scenario = null;
			ScenarioManager scenarioMng = new ScenarioManager(Mode.Right);
			Ev3System robokon = Ev3System.getInstance ();
			robokon.allResetParam();
			bool isEndScenario = false;
			int black = 5;
			int white = 50;
            
			// キャリブレーション
            #if false
            // 白
            InfoDialog whiteChk = new InfoDialog("white", true);
            whiteChk.Show();//Wait for enter to be pressed
			white = robokon.colorRead ();

            // 黒
            InfoDialog blackChk = new InfoDialog("Black", true);
            blackChk.Show ();
			black = robokon.colorRead ();

            // 灰
			robokon.TargetLight = (white + black) / 2;

			// ステアリング
			// タッチセンサーを押すまで後輪が同じパワーで動き続ける
			// 前輪の向きを調整してください
			InfoDialog steerChk = new InfoDialog("Steer", true);
			steerChk.Show();
			while (!robokon.touchIsPressed())
			{
				robokon.setLeftMotorPower(100);
				robokon.setRightMotorPower(100);
			}
			#endif
			robokon.setLeftMotorPower(0);
			robokon.setRightMotorPower(0);
			robokon.allResetParam();

			//スタート待ち
			InfoDialog dialogSTART = new InfoDialog("while=" + white + " black=" + black, true);
			dialogSTART.Show();//Wait for enter to be pressed

			// 最終的な動作開始はタッチセンサ
			while (!robokon.touchIsPressed())
			{
				// 開始待ち
			}
			System.Threading.Thread.Sleep(500);

			// 最初のシナリオを取得する
			scenario = scenarioMng.getCurrentScenario();

			// メインループ
			while (!robokon.touchIsPressed()) {
				isEndScenario = scenario.run (robokon);
				if (isEndScenario) {
					scenario = scenarioMng.updateScenario ();
					if (scenario == null) {
						break;
					}
				} else {
					Thread.Sleep (10);
				}
			}

			// 後処理
			robokon.steerMotor.Off();
			robokon.leftMotor.Off();
			robokon.rightMotor.Off();

			Lcd.Instance.Clear();
			Lcd.Instance.Update();
		}

		/// <summary>
		/// 指定したタコカウントに前輪を回転させる
		/// </summary>
		/// <param name="steer">Steer.</param>
		/// <param name="tacho">Tacho.</param>
		/// <param name="power">Power.</param>
		static void SetSteerTacho(Motor steer, int targetTacho, uint power)
		{
			int currentTacho = steer.GetTachoCount();
			int diff = targetTacho - currentTacho;

			sbyte p = (diff >= 0) ? (sbyte)power : (sbyte)(power * -1);
			uint speed = (uint)Math.Abs(diff);
			uint up = 0;
			uint down = 0;
			if (speed > 20)
			{
				up = 10;
				down = 10;
				speed -= 20;
			}
			steer.SpeedProfile(p, up, speed, down, true);
		}

		/// <summary>
		/// ライントレースのテスト
		/// </summary>
		/// <param name="light">Light.</param>
		/// <param name="grey">Grey.</param>
		/// <param name="steer">Steer.</param>
		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>
		static int LineTraceTest(int light, int grey, Motor steer, Motor left, Motor right)
		{
			int steerCnt = steer.GetTachoCount();

			#if true
			if (light > grey + LIGHT_WIDTH)
			{
				// 白い場合は右に曲がる
				if (steerCnt < MAX_STEERING_ANGLE)
				{
					steer.SetPower(STEER_POWER);
				}
				else
				{
					steer.Brake();
				}
				left.SetPower(-1 * LINE_TRACE_DRV_PWR);
				right.SetPower(0);
			}
			else if (light < grey - LIGHT_WIDTH)
			{
				// 黒い場合は左に曲がる
				if (steerCnt > -1 * MAX_STEERING_ANGLE)
				{
					steer.SetPower(-1 * STEER_POWER);
				}
				else
				{
					steer.Brake();
				}
				left.SetPower(0);
				right.SetPower(-1 * LINE_TRACE_DRV_PWR);
			}
			else
			{
				// 灰色から規定範囲内ならステアリングをとめてゆっくり進む
				steer.Brake();
				left.SetPower(-1 * LINE_TRACE_DRV_PWR / 2);
				right.SetPower(-1 * LINE_TRACE_DRV_PWR / 2);
			}
			#endif
			return 0;
		}

		/// <summary>
		/// ループ部分
		/// </summary>
		/// <param name="system">ev3System</param>
		private static int loopHandling(object system)
		{
			Ev3System robokon = (Ev3System)system;
			Monitor.Enter(robokon);

			//TBD(値は固定)
			int grey = 25;
			sbyte edg = 1;
			int count = 0;
			sbyte outPower = OUT_WHEEL_POWER;
			sbyte inPower = IN_WHEEL_POWER;

			stepKind sk = stepKind.L_CURVE;

			// 基本的なライントレース処理
			basicLineTrace(robokon, grey, edg, ref count, ref outPower, ref inPower);

			// 2015年8月10日の飯田さんのプログラムの違いは、基本的にここだけ
			if (isStep(robokon.leftMotor, robokon.rightMotor, robokon.steerMotor) == true)
			{

				switch (sk)
				{
				case stepKind.L_CURVE:  //L字用処理
					//L字想定走行
					actionLPoint(robokon);
					break;
				case stepKind.TWO_BRIDGE:
					break;
				case stepKind.BARCODE:
					break;
				default:
					break;

				}

				return -1;
			}
			return 0;

		}
		/// <summary>
		/// 基本的なライントレース処理
		/// </summary>
		/// <param name="robokon">ev3System</param>
		/// <param name="grey">灰色基準値</param>
		/// <param name="edg"></param>
		/// <param name="count"></param>
		/// <param name="outPower"></param>
		/// <param name="inPower"></param>
		private static void basicLineTrace(Ev3System robokon, int grey, sbyte edg, ref int count, ref sbyte outPower, ref sbyte inPower)
		{
			int light = robokon.colorRead();
			double pidVal = pid(light, grey);
			pidVal = (sbyte)(pidVal * edg);
			if (edg == 1)
			{
				outPower = OUT_WHEEL_POWER;
				inPower = IN_WHEEL_POWER;
			}
			else
			{
				outPower = IN_WHEEL_POWER;
				inPower = OUT_WHEEL_POWER;
			}

			count = robokon.steerGetTachoCount();
			if (light > grey)
			{

				if (count < MAX_STEERING_ANGLE)
				{
					robokon.setSteerPower((sbyte)pidVal);
				}
				else
				{
					robokon.steerBrake();
				}

				robokon.setLeftMotorPower(outPower);
				robokon.setRightMotorPower(inPower);
			}
			else
			{

				if (count > -1 * MAX_STEERING_ANGLE)
				{
					robokon.setSteerPower((sbyte)pidVal);
				}
				else
				{
					robokon.steerBrake();
				}

				robokon.setLeftMotorPower(inPower);
				robokon.setRightMotorPower(outPower);
			}
		}

		/// <summary>
		/// キャリブレーション
		/// </summary>
		/// <returns>灰色と判断する基準値</returns>
		static int calib()
		{
			int grey, black, white;
			int b_r = 0, b_a = 0;
			int w_r = 0, w_a = 0;
			int cnt = 0;
			const int sample_max = 10;		//キャリブレーションのサンプル数

			EV3ColorSensor color = new EV3ColorSensor(SensorPort.In3);
			InfoDialog dialogCalib = new InfoDialog("TEST", true);

			color.Mode = ColorMode.Ambient;

			//黒　周辺光
			dialogCalib.UpdateMessage("Balck Ambient");
			dialogCalib.Show();
			while (cnt < sample_max)
			{
				cnt = cnt + 1;
				b_a = b_a + color.Read();
			}
			cnt = 0;
			color.Mode = ColorMode.Reflection;

			//黒 反射光
			dialogCalib.UpdateMessage("Balck Reflection");
			dialogCalib.Show();
			while (cnt < sample_max)
			{
				cnt = cnt + 1;
				b_r = b_r + color.Read();
			}
			cnt = 0;
			color.Mode = ColorMode.Ambient;

			//白　周辺光
			dialogCalib.UpdateMessage("White Ambient");
			dialogCalib.Show();//Wait for enter to be pressed
			while (cnt < sample_max)
			{
				cnt = cnt + 1;
				w_a = w_a + color.Read();
			}
			cnt = 0;
			color.Mode = ColorMode.Reflection;

			//白 反射光
			dialogCalib.UpdateMessage("White Reflection");
			dialogCalib.Show();//Wait for enter to be pressed
			while (cnt < sample_max)
			{
				cnt = cnt + 1;
				w_r = w_r + color.Read();
			}

			black = (b_r / sample_max) - (b_a / sample_max);
			white = (w_r / sample_max) - (w_a / sample_max);
			grey = (black + white) / 2;

			return grey;
		}

		/// <summary>
		/// PID
		/// </summary>
		/// <param name="val"></param>
		/// <param name="targetVal"></param>
		/// <returns></returns>
		static double pid(int val, int targetVal)
		{
			double p = 0, i = 0, d = 0;
			double ret;
			double delta = (sleepTime / 1000);	//単位をmに変換

			//最初は前回値が無いのでPだけで評価
			if (isFirstPID == true)
			{
				diff[0] = val - targetVal;
				isFirstPID = false;
				p = kp * diff[0];
				ret = p;

				return ret;
			}

			diff[1] = diff[0];
			diff[0] = val - targetVal;
			integral += (double)((diff[0] + diff[1]) / 2.0 * delta);

			p = kp * diff[0];
			//i = ki * integral;
			//d = kd * (diff [0] - diff [1]) / delta;

			ret = p + i + d;

			if (ret > POWER_MAX)
			{
				ret = POWER_MAX;
			}
			else if (ret < (POWER_MAX * -1))
			{
				ret = POWER_MAX * -1;
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
		/// 黒ラインを見つけるまで前進する		Todo：黒色値（２５）を取得する方法が必要
		/// </summary>
		/// <param name="robokon">Robokon.</param>
		/// <param name="pw">Pw.</param>
		static void actionAdvanceToBlackLine(Ev3System robokon, sbyte pw)
		{
			robokon.setSteerSlope (0);
			robokon.leftMotor.SetPower((sbyte)(pw * -1));
			robokon.rightMotor.SetPower((sbyte)(pw * -1));
			while (true)
			{
				if (robokon.color.Read() < 25)
				{
					break;
				}
				//8ミリ秒待ち
				Thread.Sleep(1);
			}

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
		/// 回転する　Todo：引数が意味不明で使用方法が分からないので、人視点でのIFに修正する
		/// </summary>
		/// <param name="robokon">Robokon.</param>
		/// <param name="rightPw">Right pw.</param>
		/// <param name="leftPw">Left pw.</param>
		/// <param name="slop">Slop.</param>
		/// <param name="distance">Distance.</param>
		static void actionTurn(Ev3System robokon, sbyte rightPw, sbyte leftPw, int slop, uint distance)
		{
			int[] tacho = new int[2];
			int distanceToTacho = (int)(distance * 25); //距離をタコ回転数に変換した値。　２５回転で1cm

			robokon.setSteerSlope (slop);

			tacho[0] = robokon.leftMotorGetTachoCount();
			robokon.leftMotor.SetPower(leftPw);
			robokon.rightMotor.SetPower(rightPw);

			while (true)
			{
				tacho[1] = robokon.leftMotorGetTachoCount();
				if (tacho[1] < (tacho[0] - distanceToTacho))
				{
					break;
				}
				//8ミリ秒待ち
				Thread.Sleep(1);
			}
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

		// colorの値使ってない気がする
		static void actionLPoint(Ev3System robokon)
		{
			int grey = 25;
			sbyte edg = 2;
			int count = 0;
			sbyte outPower = OUT_WHEEL_POWER;
			sbyte inPower = IN_WHEEL_POWER;
			int[] tacho = new int[2];


			robokon.color.Mode = ColorMode.Color;

			//ステアリングの傾きを直すために、段差から少し離れる
			actionBackward(robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, 30, 4, true);

			//ステアリングの傾きを直す
			//actionSlopeChange (robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, -30);
			//actionSlopeChange (robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, 30);
			robokon.setSteerSlope(0);

			//前進して段差を超える
			actionAdvance(robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, 70, 20, true);

			//ステアリングの傾きを直す
			//actionSlopeChange (robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, -30);
			//actionSlopeChange (robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, 30);
			robokon.setSteerSlope(0);

			//ライン上に戻す
			if (robokon.colorRead() != (int)Color.Black)
			{

				robokon.setSteerSlope(-60);
				tacho[0] = robokon.rightMotorGetTachoCount();
				robokon.setLeftMotorPower(-20);
				robokon.setRightMotorPower(20);
				//左回転
				while (robokon.colorRead() != (int)Color.Black)
				{
					tacho[1] = robokon.rightMotorGetTachoCount();
					if (tacho[1] < (tacho[0] - (25 * 5)))
					{
						edg = 1;
						robokon.setSteerSlope(60);
						tacho[0] = robokon.leftMotorGetTachoCount();
						robokon.setLeftMotorPower(20);
						robokon.setRightMotorPower(-20);
						//左回転
						while (robokon.colorRead() != (int)Color.Black)
						{
							tacho[1] = robokon.leftMotorGetTachoCount();
							if (tacho[1] < (tacho[0] - (25 * 10)))
							{
								break;
							}
							Thread.Sleep(1);
						}

						break;
					}
					Thread.Sleep(1);
				}

			}
			else
			{

			}

			robokon.color.Mode = ColorMode.Reflection;
			//ステアリングの傾きを直す
			//actionSlopeChange (robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, -30);
			//actionSlopeChange (robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, 30);
			robokon.setSteerSlope(0);

			tacho[0] = robokon.leftMotorGetTachoCount();
			while (true)
			{
				basicLineTrace(robokon, grey, edg, ref count, ref outPower, ref inPower);
				tacho[1] = robokon.leftMotorGetTachoCount();
				if (tacho[1] < (tacho[0] - (25 * 7)))
				{
					break;
				}
				//8ミリ秒待ち
				Thread.Sleep(1);
			}

			//ステアリングの傾きを直す
			//actionSlopeChange (robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, -30);
			//actionSlopeChange (robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, 30);
			robokon.setSteerSlope(0);

			actionAdvance(robokon.leftMotor, robokon.rightMotor, robokon.steerMotor, 40, 10, true);

			//右に旋回  
			actionTurn(robokon, 0, -40, 60, 5);

			//黒色ラインを見つけるまで前進
			actionAdvanceToBlackLine(robokon, 20);

			//actionTurn (robokon, -20, -40, 45, 30);
			robokon.setSteerSlope(45);

			tacho[0] = robokon.leftMotorGetTachoCount();
			robokon.leftMotor.SetPower(-40);
			robokon.rightMotor.SetPower(-20);

			while (true)
			{
				tacho[1] = robokon.leftMotorGetTachoCount();
				if (tacho[1] < (tacho[0] - (30 * 25)))
				{
					break;
				}
				else if (tacho[1] < (tacho[0] - (15 * 25)))
				{
					if (robokon.colorRead() < 25)
					{
						break;
					}
				}
				//8ミリ秒待ち
				Thread.Sleep(1);
			}

		}
	}
}
