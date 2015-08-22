using System;

namespace ETrikeV
{
	public class UndeterminedAreaScenario: Scenario
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ETrikeV.UndeterminedAreaScenario"/> class.
		/// </summary>
		public UndeterminedAreaScenario ()
		{
			//規定処理は特になし
		}

		/// <summary>
		/// 仕様未確定エリア2
		/// </summary>
		/// <param name="sys">Sys.</param>
		public override bool run(Ev3System sys)
		{
			
			return true;
		}
	}
}

