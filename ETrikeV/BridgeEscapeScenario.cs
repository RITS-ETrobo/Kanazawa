using System;

namespace ETrikeV
{
	public class BridgeEscapeScenario : Scenario
	{
		private enum BridgeEscapeState {Turn1, Straight1, Turn2, Straight2, Turn3, Straight3, Turn4, Line};
		private BridgeEscapeState st = BridgeEscapeState.Turn1;

		public BridgeEscapeScenario ()
		{
		}

		public override bool run(Ev3System sys)
		{
			bool ret = false;

			return ret;
		}
	}
}

