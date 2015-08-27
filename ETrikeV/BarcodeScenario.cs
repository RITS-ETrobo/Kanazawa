using System;

namespace ETrikeV
{
	public class BarcodeScenario : Scenario
	{
		/// <summary>
		/// 読み取ったバーコード情報
		/// </summary>
		int BarcodeBit = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="ETrikeV.BarcodeScenario"/> class.
		/// </summary>
		public BarcodeScenario ()
		{
			// 何もなし
		}

		/// <summary>
		/// バーコード
		/// </summary>
		/// <param name="sys">Sys.</param>
		public override bool run(Ev3System sys)
		{
			//段差検出
			//バーコード開始位置検出(通常ライントレースしながら)
			//3cm単位でバーコード読み取り(3cm×10回)
			//終わったら、this.BarcodeBit に値を入れる
			return true;
		}

		/// <summary>
		/// バーコードの値
		/// </summary>
		/// <returns>The barcode bit.</returns>
		public int getBarcodeBit()
		{
			return BarcodeBit;
		}
	}
}

