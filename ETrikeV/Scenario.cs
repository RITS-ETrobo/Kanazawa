using System;
using MonoBrickFirmware.Movement;

namespace ETrikeV
{
	public abstract class Scenario
	{
		public abstract bool run (Ev3System robokon);

		// 基本的な走行メソッドはここに定義する //

		/// <summary>
		/// ライントレース
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="edge">Edge.</param>
		/// <param name="margin">Margin.</param>
		/// <param name="maxAngle">Max angle.</param>
		/// <param name="steerPwr">Steer pwr.</param>
		protected void lineTrace(Ev3System sys, int speed, Mode edge, int margin, int maxAngle, int steerPwr)
		{
			int light = sys.colorRead ();
			int steerCnt = sys.steerGetTachoCount ();
			if (edge == Mode.Left) {
				if (light > sys.TargetLight + margin) {
					// 白い場合は右に曲がる
					if (steerCnt < maxAngle) {
						sys.setSteerPower (steerPwr);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (0);
				} else if (light < sys.TargetLight - margin) {
					// 黒い場合は左に曲がる
					if (steerCnt > -1 * maxAngle) {
						sys.setSteerPower (-1 * steerPwr);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (0);
					sys.setRightMotorPower (speed);
				} else {
					// 灰色から規定範囲内ならステアリングをとめてゆっくり進む
					sys.steerBrake ();
					sys.setLeftMotorPower (speed / 2);
					sys.setRightMotorPower (speed / 2);
				}
			} else {
				if (light > sys.TargetLight + margin) {
					// 白い場合は左に曲がる
					if (steerCnt > -1 * maxAngle) {
						sys.setSteerPower (-1 * steerPwr);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (0);
					sys.setRightMotorPower (speed);
				} else if (light < sys.TargetLight - margin) {
					// 黒い場合は右に曲がる
					if (steerCnt < maxAngle) {
						sys.setSteerPower (steerPwr);
					} else {
						sys.steerBrake ();
					}
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (0);
				} else {
					// 灰色から規定範囲内ならステアリングをとめてゆっくり進む
					sys.steerBrake ();
					sys.setLeftMotorPower (speed / 2);
					sys.setRightMotorPower (speed / 2);
				}
			}
		}

		/// <summary>
		/// ストレート
		/// </summary>
		/// <param name="sys">Sys.</param>
		/// <param name="speed">Speed.</param>
		/// <param name="edge">Edge.</param>
		/// <param name="margin">Margin.</param>
		protected void straightTrace(Ev3System sys, int speed, Mode edge, int margin)
		{
			// 後輪を制御してライン左エッジを走行する
			// ステアリングは一切変えない
			int light = sys.colorRead ();
			if (edge == Mode.Left) {
				if (light > sys.TargetLight + margin) {
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (speed / 2);
				} else if (light < sys.TargetLight - margin) {
					sys.setLeftMotorPower (speed / 2);
					sys.setRightMotorPower (speed);
				} else {
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (speed);
				}
			} else {
				if (light > sys.TargetLight + margin) {
					sys.setLeftMotorPower (speed / 2);
					sys.setRightMotorPower (speed);
				} else if (light < sys.TargetLight - margin) {
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (speed / 2);
				} else {
					sys.setLeftMotorPower (speed);
					sys.setRightMotorPower (speed);
				}
			}
		}
	}
}

