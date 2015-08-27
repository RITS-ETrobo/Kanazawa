using System;
using System.Threading;
using MonoBrickFirmware;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Display.Dialogs;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using MonoBrickFirmware.Sound;

namespace ETrikeV
{
	class Program
	{

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
			//Speaker sp = new Speaker (100);
            
			// キャリブレーション
            #if true
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
					//sp.Buzz (100);
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
	}
}
