using System;

namespace ETrikeV
{
	public class LineTraceScenario : Scenario
	{
		private const int MAX_STEERING_ANGLE = 180;
		private const int STEER_POWER = 100;
		private const int LIGHT_WIDTH = 8; //10
		private int endTachoCount;
		private int speed;
		private Mode edge;

		public LineTraceScenario (int endTachoCount, int speed, Mode edge)
		{
			this.endTachoCount = endTachoCount;
			this.speed = speed;
			this.edge = edge;
		}

		public override bool run(Ev3System sys)
		{
			// 終了確認
			if (sys.getAverageTachoCount() > endTachoCount) {
				sys.setSteerSlope (0);
				return true;
			}

			// ライントレース
			lineTrace (sys, speed, edge, LIGHT_WIDTH, MAX_STEERING_ANGLE, STEER_POWER);

			return false;
		}
	}
}

