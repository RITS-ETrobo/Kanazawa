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
			int startPos = sys.getAverageMoveCM ();
			sys.setBackMotorsSpeed (-100);
			while (startPos - sys.getAverageMoveCM () < 10) {
				
			}
			sys.setBackMotorsSpeed (100);
			System.Threading.Thread.Sleep (1000);

			return true;
		}
	}
}

