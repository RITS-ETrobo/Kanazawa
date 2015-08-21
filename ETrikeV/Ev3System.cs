using System;
using MonoBrickFirmware;
using MonoBrickFirmware.Display.Dialogs;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using System.Threading;
using MonoBrickFirmware.Sensors;


namespace ETrikeV
{
    /// <summary>
    /// コースの攻略に関係ない部分
    /// EV3本体については、このクラスが責務を持つ
    /// </summary>
    public class Ev3System
    {
		private static Ev3System instance = new Ev3System();
		public int TargetLight { get; set; }

		private Ev3System() {
			initialize ();
		}

        /// <summary>
        /// システムの初期化
        /// </summary>
        /// <returns>初期化の成否</returns>
        private void initialize()
		{
            color = new EV3ColorSensor(SensorPort.In3, ColorMode.Reflection);
            touch = new EV3TouchSensor(SensorPort.In2);
            sonic = new EV3UltrasonicSensor(SensorPort.In1);
            gyro = new EV3GyroSensor(SensorPort.In4);

            steerMotor = new Motor(MotorPort.OutC);
            leftMotor = new Motor(MotorPort.OutA);
            rightMotor = new Motor(MotorPort.OutB);
            gearRatio = 25; // 一番低いギア比はこの値

            // 止めておく
            steerMotor.Off();
            leftMotor.Off();
            rightMotor.Off();
        }

		public static Ev3System getInstance()
		{
			return instance;
		}

        /// <summary>
        /// タコメータ、ジャイロなど全てのリセット
        /// </summary>
        public void allResetParam()
        {
            tachometerReset();
            gyroReset();

            return;
        }

        /// <summary>
        /// タコメータの値初期化
        /// </summary>
        public void tachometerReset()
        {
            leftMotor.ResetTacho();
            steerMotor.ResetTacho();
            rightMotor.ResetTacho();
            return;
        }

        /// <summary>
        /// ジャイロセンサーの値初期化
        /// </summary>
        public void gyroReset()
        {
            gyro.Reset();
            return;
        }

        /// <summary>
        /// カラーセンサーの読み取り
        /// </summary>
        /// <returns></returns>
        public int colorRead()
        {
            return color.Read();
        }

        /// <summary>
        /// ステアリングモーター:power
        /// </summary>
        /// <param name="power"></param>
		public void setSteerPower(int power)
        {
			steerMotor.SetPower((sbyte)power);
        }
        /// <summary>
        /// ステアリングモーター:TachoCount
        /// </summary>
        /// <returns></returns>
        public int steerGetTachoCount()
        {
            return steerMotor.GetTachoCount();
        }
        /// <summary>
        /// ステアリングモーター:Brake
        /// </summary>
        /// <returns></returns>
        public void steerBrake()
        {
            steerMotor.Brake();
            return;
        }
        /// <summary>
        /// ステアリングモーター:off
        /// </summary>
        /// <returns></returns>
        public void steerOff()
        {
            steerMotor.Off();
            return;
        }

        /// <summary>
        /// 左モーター:power
        /// </summary>
        /// <param name="power">モーターパワー:前進は正数</param>
        public void setLeftMotorPower(int power)
        {
            leftMotor.SetPower((sbyte)(power * -1));
        }
        /// <summary>
        /// 左モーター:TachoCount
        /// </summary>
        /// <returns></returns>
        public int leftMotorGetTachoCount()
        {
            return leftMotor.GetTachoCount() * -1;
        }

        /// <summary>
        /// 左モーター:移動距離(cm)
        /// </summary>
        /// <returns></returns>
        public int leftMotorGetMoveCm()
        {
			return (leftMotorGetTachoCount() / gearRatio);
        }

        /// <summary>
        /// 左モーター:Brake
        /// </summary>
        /// <returns></returns>
        public void leftMotorBrake()
        {
            leftMotor.Brake();
            return;
        }
        /// <summary>
        /// 左モーター:off
        /// </summary>
        /// <returns></returns>
        public void leftMotorOff()
        {
            leftMotor.Off();
            return;
        }

        /// <summary>
        /// 右モーター:power
        /// </summary>
        /// <param name="power">モーターパワー:前進は正数</param>
		public void setRightMotorPower(int power)
        {
            rightMotor.SetPower((sbyte)(power * -1));
        }
        /// <summary>
        /// 右モーター:TachoCount
        /// </summary>
        /// <returns></returns>
        public int rightMotorGetTachoCount()
        {
            return rightMotor.GetTachoCount() * -1;
        }
        /// <summary>
        /// 右モーター:移動距離(cm)
        /// </summary>
        /// <returns></returns>
        public int rightMotorGetMoveCm()
        {
			return (rightMotorGetTachoCount() / gearRatio);
        }
        /// <summary>
        /// 右モーター:Brake
        /// </summary>
        /// <returns></returns>
        public void rightMotorBrake()
        {
            rightMotor.Brake();
            return;
        }
        /// <summary>
        /// 右モーター:off
        /// </summary>
        /// <returns></returns>
        public void rightMotorOff()
        {
            rightMotor.Off();
            return;
        }

        /// <summary>
        /// ジャイロの読み取り
        /// </summary>
        /// <returns></returns>
        public int getGyro()
        {
            return gyro.Read();
        }

        /// <summary>
        /// タッチセンサ
        /// </summary>
        /// <returns>押されているかどうか</returns>
        public bool touchIsPressed()
        {
            return touch.IsPressed();
        }

		/// <summary>
		/// 指定した角度に前輪を向ける
		/// </summary>
		/// <param name="slope">Slope.</param>
		public void setSteerSlope(int slope)
		{
			int tacho;
			int maxSlopeRange = 5;
			int slopeToTacho = slope * 8;

			//停止
			leftMotor.Brake();
			rightMotor.Brake ();
			steerMotor.Brake ();

			tacho = steerMotor.GetTachoCount();

			//前輪を真っ直ぐに治す
			while (true)
			{
				tacho = steerMotor.GetTachoCount();
				if ((tacho <= (slopeToTacho + maxSlopeRange)) && (tacho >= (slopeToTacho - maxSlopeRange)))
				{
					steerMotor.Brake ();
					break;
				}

				if (tacho > (slopeToTacho + maxSlopeRange))
				{
					steerMotor.SetPower(-100);
				}
				else
				{
					steerMotor.SetPower(100);
				}
			}

			// 微調整
			while (true) {
				tacho = steerMotor.GetTachoCount();
				if ((tacho <= (slopeToTacho + maxSlopeRange)) && (tacho >= (slopeToTacho - maxSlopeRange))) {
					steerMotor.Brake ();
					break;
				}

				if (tacho > (slopeToTacho + maxSlopeRange)) {
					steerMotor.SetPower(-50);
				} else {
					steerMotor.SetPower(50);
				}
			}
		}

        // システムのハードウエアのもの
        public EV3ColorSensor color;
        protected EV3TouchSensor touch;
        protected EV3UltrasonicSensor sonic;
        public EV3GyroSensor gyro;

        //TBD 後でprotectedに変更する
        //protected Motor steerMotor;
        //protected Motor leftMotor;
        //protected Motor rightMotor;
        public Motor steerMotor;
        public Motor leftMotor;
        public Motor rightMotor;
        protected int gearRatio;
    }

}
