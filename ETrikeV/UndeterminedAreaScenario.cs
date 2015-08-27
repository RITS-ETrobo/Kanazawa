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
			//いやだけど、ここでバーコードの情報をもらわないとどうにもらなない
			BarcodeScenario barcode = new BarcodeScenario();
			barcode.run (sys);
			int barcodeBit = barcode.getBarcodeBit ();

			//ここから、仕様未確定エリアの動作


			return true;
		}
	}
}

