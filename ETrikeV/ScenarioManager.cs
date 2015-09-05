using System;
using System.Collections.Generic;

namespace ETrikeV
{
	public class ScenarioManager
	{
		private List<Scenario> scenarioList = null;
		private int currentScenarioNo = 0;

		public ScenarioManager (Mode mode)
		{
			scenarioList = new List<Scenario> ();
			if (mode == Mode.Left) {
				// Lコース
				scenarioList.Add (new StraightScenario (  1600,  80, Mode.Right, true));
				scenarioList.Add (new RightAngleParkScenario ());
				scenarioList.Add (new LineTraceScenario ( 1900,  60, Mode.Right));
				scenarioList.Add (new StraightScenario (  6700, 100, Mode.Right, true));	//6800
				scenarioList.Add (new StraightScenario (  7000,  50, Mode.Right));
				scenarioList.Add (new CornerScenario (    8300,  25, 50, 40, Mode.Right));
				scenarioList.Add (new LineTraceScenario ( 8700,  60, Mode.Right));
				scenarioList.Add (new StraightScenario ( 10700,  80, Mode.Right, true));
				#if true
				scenarioList.Add (new BridgeScenario ());
				#else
				scenarioList.Add(new BridgeEscapeScenario(Mode.Right, Mode.Right));
				#endif
				scenarioList.Add (new LineTraceScenario (13000, 80, Mode.Right));
				scenarioList.Add (new StraightScenario ( 13200, 50, Mode.Right, false));
				#if false
				scenarioList.Add (new UndeterminedAreaScenario ());
				scenarioList.Add (new LineTraceScenario (26000, 80, Mode.Right));
				//scenarioList.Add (new RightAngleParkScenario (  8700,  80, Mode.Right));
				#else
				scenarioList.Add (new BarcodeScenario());
				scenarioList.Add (new ShortcutScenario());
				scenarioList.Add (new LineTraceScenario (20000, 70, Mode.Left)); // Leftで間違っていません
				scenarioList.Add (new StraightScenario ( 26000, 100, Mode.Left, true)); // Leftで間違っていません
				#endif

			} else {
				// Rコース
				#if true
				scenarioList.Add (new StraightScenario(   5800, 100, Mode.Left, true));		// 最初の直線1
				scenarioList.Add (new StraightScenario(   6100,  50, Mode.Left));			// 最初の直線2
				scenarioList.Add (new CornerScenario (    7200,  25, 60, -30, Mode.Left));	// ヘアピン
				scenarioList.Add (new LineTraceScenario ( 7500,  50, Mode.Left));			// ヘアピン後の角度補正
				scenarioList.Add (new StraightScenario   (8700,  80, Mode.Left, true));
				scenarioList.Add (new LineTraceScenario (10900,  60, Mode.Left));
				scenarioList.Add (new StraightScenario  (12660,  80, Mode.Left, true));
				scenarioList.Add (new LineTraceScenario (13900,  70, Mode.Left));	//13700
				scenarioList.Add (new StraightScenario  (13950,  40, Mode.Left));	//14000
				scenarioList.Add (new FigureLScenario  ());
				scenarioList.Add (new ShinkansenScenario ());
				scenarioList.Add (new LineTraceScenario (18500,  70, Mode.Left));
				scenarioList.Add (new StraightScenario(  19900,  60, Mode.Left, true));
				scenarioList.Add (new LineTraceScenario (21500,  80, Mode.Left));
				scenarioList.Add (new StraightScenario(  26525, 100, Mode.Left, true));
				scenarioList.Add (new JuretsuParkScenario());
				#else
				scenarioList.Add(new TestScenario());
				#endif
			}
		}

		/// <summary>
		/// 現在のシナリオを取得する
		/// </summary>
		/// <returns>The current scenario.</returns>
		public Scenario getCurrentScenario() {
			return this.scenarioList [currentScenarioNo];
		}

		/// <summary>
		/// 次のシナリオへ変更する
		/// </summary>
		/// <returns>新しいシナリオ</returns>
		public Scenario updateScenario() {
			this.currentScenarioNo++;
			if (this.scenarioList.Count == this.currentScenarioNo) {
				return null;
			} else {
				return this.scenarioList [this.currentScenarioNo];
			}
		}
	}
}

