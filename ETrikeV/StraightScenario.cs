using System;

namespace ETrikeV
{
	public class StraightScenario : Scenario
	{
		private const int LIGHT_WIDTH = 8;
		private int endTachoCount;
		private int targetSpeed;
		private int currentSpeed = 0;
		private Mode edge;
		private bool smooth;

		public StraightScenario (int endTachoCount, int speed, Mode edge, bool smooth = false)
		{
			this.endTachoCount = endTachoCount;
			targetSpeed = speed;
			this.edge = edge;
			this.smooth = smooth;
		}

		public override bool run(Ev3System sys)
		{
			// 終了確認
			if (sys.getAverageTachoCount() > endTachoCount) {
				return true;
			}

			if (smooth) {
				if (currentSpeed < targetSpeed) {
					currentSpeed++;
				}
			} else {
				currentSpeed = targetSpeed;
			}

			straightTrace (sys, currentSpeed, edge, LIGHT_WIDTH);

			return false;
		}
	}
}

