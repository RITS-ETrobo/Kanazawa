using System;

namespace ETrikeV
{
	public class TestScenario : Scenario
	{
		public TestScenario ()
		{
		}

		public override bool run(Ev3System sys)
		{
			sys.setSteerSlope (-45);
			System.Threading.Thread.Sleep (1000);
			sys.setSteerSlope (45);
			System.Threading.Thread.Sleep (1000);
			sys.setSteerSlope (0);
			System.Threading.Thread.Sleep (1000);


			return false;
		}
	}
}

