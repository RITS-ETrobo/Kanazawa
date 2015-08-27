using System;

namespace ETrikeV
{
	public class StraightScenario : Scenario
	{
		private const int LIGHT_WIDTH = 10;
		private int endTachoCount;
		private int speed;
		private Mode edge;

		public StraightScenario (int endTachoCount, int speed, Mode edge)
		{
			this.endTachoCount = endTachoCount;
			this.speed = speed;
			this.edge = edge;
		}

		public override bool run(Ev3System sys)
		{
			// 終了確認
			if (sys.getAverageTachoCount() > endTachoCount) {
				return true;
			}

			straightTrace (sys, speed, edge, LIGHT_WIDTH);

			return false;
		}
	}
}

