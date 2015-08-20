using System;
using MonoBrickFirmware.Movement;

namespace ETrikeV
{
	public class Scenario
	{
		public Mode Mode;
		public int EndTachoCnt;
		public int Speed;
		public int StartSteerPos;

		public Scenario (Mode mode, int endTachoCnt, int speed, int startSteerPos)
		{
			this.Mode = mode;
			this.EndTachoCnt = endTachoCnt;
			this.Speed = speed;
			this.StartSteerPos = startSteerPos;
		}

		public bool isEndScenario(Motor left, Motor right)
		{
			if ((left.GetTachoCount () + right.GetTachoCount ()) / 2 < (this.EndTachoCnt * -1)) {
				return true;
			} else {
				return false;
			}
		}
	}
}

