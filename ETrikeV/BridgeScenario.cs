using System;

namespace ETrikeV
{
	public class BridgeScenario : Scenario
	{
		private enum State {Line, Back, Go, Jump, End}
		private State state = State.Line;
		private int prevTacho = 0;
		private int stopCount = 0;
		private int stepPosCount = 0;

		public BridgeScenario ()
		{
		}

		public override bool run(Ev3System sys)
		{
			bool ret = false;

			switch (state) {
			case State.Line:
				state = line (sys);
				break;
			case State.Back:
				state = back (sys);
				break;
			case State.Go:
				state = go (sys);
				break;
			case State.Jump:
				state = jump (sys);
				break;
			case State.End:
				ret = true;
				break;
			default:
				break;
			}

			return ret;
		}

		/// <summary>
		/// 段差を検知するまでライントレースする
		/// 3回同じタコを検知したら段差と判断する
		/// </summary>
		/// <param name="sys">Sys.</param>
		private State line(Ev3System sys) {
			int tacho = sys.getAverageTachoCount();
			if (prevTacho == tacho) {
				stopCount++;
			} else {
				prevTacho = tacho;
				stopCount = 0;
			}

			if (stopCount > 2) {
				sys.setSteerSlope (0);
				stopCount = 0;
				return State.Back;
			}

			lineTrace(sys, 50, Mode.Right, 10, 100, 100);

			return State.Line;
		}

		/// <summary>
		/// 10cm下がる
		/// </summary>
		/// <param name="sys">Sys.</param>
		private State back(Ev3System sys)
		{
			if (stepPosCount == 0) {
				stepPosCount = sys.getAverageMoveCM ();
			} else {
				int tachoCm = sys.getAverageMoveCM ();
				if (stepPosCount - tachoCm > 5) {
					//sys.backMotorsBrake ();
					sys.setBackMotorsSpeed (100);
					return State.Go;
				}
			}

			sys.setBackMotorsSpeed (-100);

			return State.Back;
		}

		/// <summary>
		/// 2本橋を渡る
		/// </summary>
		/// <param name="sys">Sys.</param>
		private State go(Ev3System sys)
		{
			int tachoCm = sys.getAverageMoveCM ();
			if (tachoCm - stepPosCount > 20) {
				sys.backMotorsBrake ();
				return State.Jump;
			}
			#if false
			if (tachoCm == prevTacho) {
				stopCount++;
				if (stopCount > 10) {
					stepPosCount = 0;
					stopCount = 0;
					return State.Back;
				}
			} else {
				prevTacho = tachoCm;
				stopCount = 0;
			}
			#endif

			sys.setBackMotorsSpeed (100, 0);

			return State.Go;
		}

		/// <summary>
		/// ジャンプする
		/// </summary>
		/// <param name="sys">Sys.</param>
		private State jump(Ev3System sys)
		{
			int tachoCm = sys.getAverageMoveCM ();
			if (tachoCm - stepPosCount > 50) {
				sys.stopMotors ();
				return State.End;
			}

			straightTrace (sys, 80, Mode.Right, 10);

			return State.Jump;
		}
	}
}

