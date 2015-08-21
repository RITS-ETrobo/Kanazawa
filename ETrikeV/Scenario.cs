using System;
using MonoBrickFirmware.Movement;

namespace ETrikeV
{
	public abstract class Scenario
	{
		public abstract bool run (Ev3System robokon);
	}
}

