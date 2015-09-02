using System;
using System.Threading;

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

		//private int[] motionList; //値は仮 

		/// <summary>
		/// Initializes a new instance of the <see cref="ETrikeV.UndeterminedAreaScenario"/> class.
		/// </summary>
		public UndeterminedAreaScenario()
		{
			// コンストラクタでメモリ確保はしたくないが、
			// 直前だとディレイがありそうなので、事前に生成しておく
			theBarcode = new BarcodeScenario();
			//motionList = new int[5]; //値は仮 

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

			/*
			仕様未確定エリアのマス
			マスのサイズは260mm

			        ←進行方向
			      ┌────
			┌──┼──┐
			│││││││
			│┼┼┼┼┼│
			│┼┼┼┼┼│
			│┼┼┼┼┼│
			│┼┼┼┼┼│
			└─┼───┘
			    │

			*/
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

			/* 使わないのでコメントアウト
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
            */

			//提出したアルゴリズムは面倒なので、簡単な方式にする。

			// 入る前のタコを取得する
			//int leftStartCount = sys.leftMotorGetTachoCount();
			//int rightStartCount = sys.rightMotorGetTachoCount();



			//マスのあいている場所を探す
			if (cell2 != 0x00 || cell1 != 0x00)
			{
				if (cell2 != 0x00)
				{
					//ちょっとバックする
					actionStraight(sys, 15, 50);
				}
				//左に行く
				//ぶつかるまでまっすぐ行く
				//ぶつかったら右へ行く
				//次にぶつかったら左へいく
				//を繰り返す
			}
			else if (cell3 != 0x00 || cell4 != 0x00)
			{
				if (cell3 != 0x00)
				{
					//ちょっと進む
				}
				//右に行く
				//ぶつかるまでまっすぐ行く
				//ぶつかったら左へ行く
				//次にぶつかったら右へいく
				//を繰り返す
			}

			return;
		}


		/// <summary>
		/// 右に曲がる(Lシナリオからコピー)
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="rightPw">Right pw.</param>
		/// <param name="leftPw">Left pw.</param>
		/// <param name="distance">Distance.</param>
		private void actionRightTurn(Ev3System sys, sbyte rightPw, sbyte leftPw, uint distance)
		{
			int[] tmpDistance = new int[2];
			int slope = 0;

			sys.stopMotors ();

			sys.setSteerSlope (slope);

			tmpDistance[0] = sys.leftMotorGetMoveCm();
			if (rightPw == 0) {
				sys.rightMotorBrake ();
			} else {
				sys.setRightMotorPower (rightPw);
			}
			if (leftPw == 0) {
				sys.leftMotorBrake ();
			} else {
				sys.setLeftMotorPower (leftPw);
			}

			while (true)
			{
				tmpDistance[1] = sys.leftMotorGetMoveCm();
				if (tmpDistance[1] > (tmpDistance[0] + distance))
				{
					break;
				}
				//8ミリ秒待ち
				Thread.Sleep(8);
			}

			sys.stopMotors ();
		}

		/// <summary>
		/// 左に曲がる
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="rightPw">Right pw.</param>
		/// <param name="leftPw">Left pw.</param>
		/// <param name="distance">Distance.</param>
		private void actionLeftTurn(Ev3System sys, sbyte rightPw, sbyte leftPw, uint distance)
		{
			int[] tmpDistance = new int[2];
			int slope = 0;

			sys.stopMotors ();

			sys.setSteerSlope (slope);

			tmpDistance[0] = sys.rightMotorGetMoveCm();
			if (rightPw == 0) {
				sys.rightMotorBrake ();
			} else {
				sys.setRightMotorPower (rightPw);
			}
			if (leftPw == 0) {
				sys.leftMotorBrake ();
			} else {
				sys.setLeftMotorPower (leftPw);
			}

			while (true)
			{
				tmpDistance[1] = sys.rightMotorGetMoveCm();
				if (tmpDistance[1] > (tmpDistance[0] + distance))
				{
					break;
				}
				//8ミリ秒待ち
				Thread.Sleep(8);
			}

			sys.stopMotors ();
		}
	}
}

