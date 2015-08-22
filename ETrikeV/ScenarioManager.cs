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
				scenarioList.Add (new StraightScenario (  7000, 100, Mode.Right));
				scenarioList.Add (new CornerScenario (    8200,  70, 40, Mode.Right));
				scenarioList.Add (new StraightScenario (  8700,  80, Mode.Right));

			} else {
				// Rコース
				//scenarioList.Add(new TestScenario());

				scenarioList.Add (new StraightScenario(   6100, 100, Mode.Left));
				scenarioList.Add (new CornerScenario (    7350,  70, -30, Mode.Left));
				scenarioList.Add (new StraightScenario   (8700,  80, Mode.Left));
				scenarioList.Add (new LineTraceScenario (10900,  70, Mode.Left));
				scenarioList.Add (new StraightScenario  (12700,  80, Mode.Left));
				scenarioList.Add (new LineTraceScenario (13700,  70, Mode.Left));
				scenarioList.Add (new StraightScenario  (14000,  80, Mode.Left));

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

