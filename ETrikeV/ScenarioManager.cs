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
				//scenarioList.Add (new RightAngleParkScenario (  1000,  80, Mode.Right));
				scenarioList.Add (new StraightScenario (  6900, 100, Mode.Right));	//6800
				scenarioList.Add (new CornerScenario (    8400,  40, 100, 40, Mode.Right));
				scenarioList.Add (new StraightScenario ( 11000,  80, Mode.Right));
				#if true
				scenarioList.Add (new BridgeScenario ());
				#else
				scenarioList.Add(new BridgeEscapeScenario(Mode.Right, Mode.Right));
				#endif
				scenarioList.Add (new LineTraceScenario (15000, 80, Mode.Right));
				scenarioList.Add (new UndeterminedAreaScenario ());
				scenarioList.Add (new LineTraceScenario (26000, 80, Mode.Right));
				//scenarioList.Add (new RightAngleParkScenario (  8700,  80, Mode.Right));

			} else {
				// Rコース
				#if true
				scenarioList.Add (new StraightScenario(    500,  50, Mode.Left));			// 最初の直線1
				scenarioList.Add (new StraightScenario(   5800, 100, Mode.Left));			// 最初の直線2
				scenarioList.Add (new StraightScenario(   6100,  50 ,Mode.Left));			// 最初の直線3
				scenarioList.Add (new CornerScenario (    7200,  25, 60, -30, Mode.Left));	// ヘアピン
				scenarioList.Add (new LineTraceScenario ( 7500,  40, Mode.Left));			// ヘアピン後の角度補正
				scenarioList.Add (new StraightScenario   (8700,  80, Mode.Left));
				scenarioList.Add (new LineTraceScenario (10900,  50, Mode.Left));
				scenarioList.Add (new StraightScenario  (12660,  80, Mode.Left));
				scenarioList.Add (new LineTraceScenario (13800,  70, Mode.Left));	//13700
				scenarioList.Add (new StraightScenario  (13950,  40, Mode.Left));	//14000
				scenarioList.Add (new FigureLScenario  ());
				scenarioList.Add (new ShinkansenScenario ());
				scenarioList.Add (new LineTraceScenario (22000,  60, Mode.Left));
				scenarioList.Add (new StraightScenario(  26000,  80, Mode.Left));
				#else
				scenarioList.Add(new TestScenario());
				#endif
//				this.scenarioList.Add (new Scenario (Mode.Straight,  6100, 100,    0));
//				this.scenarioList.Add (new Scenario (Mode.Corner,    7350,   0, -200));
//				this.scenarioList.Add (new Scenario (Mode.Straight,  8700,  80,    0));
//				this.scenarioList.Add (new Scenario (Mode.Line,     10900,   0,    0));
//				this.scenarioList.Add (new Scenario (Mode.Straight, 12700,  80,    0));
//				this.scenarioList.Add (new Scenario (Mode.Line,     13700,   0,    0));
//				this.scenarioList.Add (new Scenario (Mode.Straight, 14000,  50,    0));
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

