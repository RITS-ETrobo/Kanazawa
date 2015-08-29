using System;

namespace ETrikeV
{
	/// <summary>
	/// 仕様未確定エリア(バーコードシナリオを内包)
	/// </summary>
	public class UndeterminedAreaScenario : Scenario
	{
		/// <summary>
		/// バーコード用のシナリオ
		/// </summary>
		private BarcodeScenario theBarcode;

		private int[] motionList; //値は仮 

		/// <summary>
		/// Initializes a new instance of the <see cref="ETrikeV.UndeterminedAreaScenario"/> class.
		/// </summary>
		public UndeterminedAreaScenario()
		{
			// コンストラクタでメモリ確保はしたくないが、
			// 直前だとディレイがありそうなので、事前に生成しておく
			theBarcode = new BarcodeScenario();
			motionList = new int[5]; //値は仮 

		}

		/// <summary>
		/// 仕様未確定エリア2
		/// </summary>
		/// <param name="sys">Sys.</param>
		public override bool run(Ev3System sys)
		{
			//いやだけど、ここでバーコードの情報をもらわないとどうにもらなない
			theBarcode.run(sys);

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

			//最下位ビットは要らないので削除
			barcodeBit = barcodeBit >> 1;

			//マスの障害物情報
			//マスごとの障害物の位置取得
			int cell1 = (barcodeBit & 0x3);
			barcodeBit = barcodeBit >> 2;
			int cell2 = (barcodeBit & 0x3);
			barcodeBit = barcodeBit >> 2;
			int cell3 = (barcodeBit & 0x3);
			barcodeBit = barcodeBit >> 2;
			int cell4 = (barcodeBit & 0x3);

			//優先コースの重み付け計算
			//コースごとの差を見る
			int courseA = System.Math.Abs(cell1 - cell2);
			int courseB = System.Math.Abs(cell2 - cell3);
			int courseC = System.Math.Abs(cell3 - cell4);

			//後でEnum化したい
			int prioritCourse = 0;
			// 本当はソートしたいけど、面倒なので、簡易的に
			// なるべく真ん中よりのコースを選ばれるようにする
			if (courseA < courseC)
			{
				if (courseC < courseB)
				{
					//優先はコースB
					prioritCourse = 2;
				}
				else
				{
					//優先はコースA
					prioritCourse = 1;
				}
			}
			else
			{
				if (courseA < courseB)
				{
					//優先はコースB
					prioritCourse = 2;
				}
				else
				{
					//優先はコースC
					prioritCourse = 3;
				}

			}

			//後でEnum化したい
			//左:1
			//まっすぐ:2
			//右:3
			//終わり:0
			motionList.Initialize();
			//if (prioritCourse == 1)
			{
				//2に入れない
				if (cell2 == 0x00)
				{
					//3に入れない
					if (cell3 == 0x00)
					{
						motionList[0] = 2;
						motionList[1] = 1;
						motionList[2] = 2;
						motionList[3] = 2;
					}
					//仕方ないから4から入る
					else
					{
						motionList[0] = 3;
						motionList[1] = 1;
						motionList[2] = 2;
						motionList[3] = 2;
					}
				}
			}

			return;
		}
	}
}

