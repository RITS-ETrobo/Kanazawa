using System;

namespace ETrikeV
{
	/// <summary>
	/// 仕様未確定エリア(バーコードシナリオを内包)
	/// </summary>
	public class UndeterminedAreaScenario: Scenario
	{
		/// <summary>
		/// バーコード用のシナリオ
		/// </summary>
		private BarcodeScenario theBarcode;

		/// <summary>
		/// Initializes a new instance of the <see cref="ETrikeV.UndeterminedAreaScenario"/> class.
		/// </summary>
		public UndeterminedAreaScenario ()
		{
			// コンストラクタでメモリ確保はしたくないが、
			// 直前だとディレイがありそうなので、事前に生成しておく
			theBarcode = new BarcodeScenario();
		}

		/// <summary>
		/// 仕様未確定エリア2
		/// </summary>
		/// <param name="sys">Sys.</param>
		public override bool run(Ev3System sys)
		{
			//いやだけど、ここでバーコードの情報をもらわないとどうにもらなない
			theBarcode.run (sys);

			//ここから、仕様未確定エリアの動作

			//ルート生成
			createUndeterminedAreaRoot(sys);

			//走行

			//ライン復帰

			return true;
		}

		/// <summary>
		/// 仕様未確定エリアのルート生成
		/// </summary>
		/// <param name="sys"></param>
		private void createUndeterminedAreaRoot(Ev3System sys)
		{
			int barcodeBit = theBarcode.getBarcodeBit();

			return;
		}
	}
}

