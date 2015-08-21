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
        //ev3System()
        //{
        //    //とりあえず、何もしない
        //    //C#がコンストラクタでnewしてもいいなら、ここで初期化処理までしてしまう。
        //    // initialize();
        //}

        /// <summary>
        /// システムの初期化
        /// </summary>
        /// <returns>初期化の成否</returns>
        public bool initialize()
        {
            bool retval = false;

            try
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

                retval = true;
            }
            catch
            {
                retval = false;
            }

            return retval;
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
        public void setSteerPower(sbyte power)
        {
            steerMotor.SetPower(power);
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
        public void setLeftMotorPower(sbyte power)
        {
            leftMotor.SetPower((sbyte)(power * -1));
        }
        /// <summary>
        /// 左モーター:TachoCount
        /// </summary>
        /// <returns></returns>
        public int leftMotorGetTachoCount()
        {
            return leftMotor.GetTachoCount();
        }

        /// <summary>
        /// 左モーター:移動距離(cm)
        /// </summary>
        /// <returns></returns>
        public int leftMotorGetMoveCm()
        {
            return (leftMotor.GetTachoCount() / gearRatio);
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
        public void setRightMotorPower(sbyte power)
        {
            rightMotor.SetPower((sbyte)(power * -1));
        }
        /// <summary>
        /// 右モーター:TachoCount
        /// </summary>
        /// <returns></returns>
        public int rightMotorGetTachoCount()
        {
            return rightMotor.GetTachoCount();
        }
        /// <summary>
        /// 右モーター:移動距離(cm)
        /// </summary>
        /// <returns></returns>
        public int rightMotorGetMoveCm()
        {
            return (rightMotor.GetTachoCount() / gearRatio);
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
