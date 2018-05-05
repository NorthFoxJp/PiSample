using System;
using System.Diagnostics;

using PiAccessLib;

namespace SampleAppMain
{
	public class SampleApp
    {
		/// <summary>
        /// コンストラクタ
        /// </summary>
		public SampleApp()
		{
		}

        /// <summary>
        /// メイン
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
		public void Main(string[] args)
		{
			try
			{
				/*
                // カメラのスナップショット
				String filePath = "/home/pi/camera/snap.jpg";
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = "raspistill";
                processStartInfo.Arguments = String.Format("-o {0}", filePath);

                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = false;

                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;

                Process process = Process.Start(processStartInfo);
                process.WaitForExit();

				if (BCM2835Managed.bcm2835_init())
                    Console.WriteLine("Initialized in full mode");
                else
                    Console.WriteLine("Initialized in GPIO mode");
				*/

				// LPS331AP - Address=0x5c
				using(BCM2835Basic bcm2835 = new BCM2835Basic())
				using(PiI2C piI2c = new PiI2C(10000, 0x5c))
				{
					// 1Hzの出力レートで動作を開始する
					piI2c.SetRegister8(0x20, 0x90);

					while (true)
					{
						// データが有効になるのを待つ
						while ((piI2c.GetRegister8(0x27) & 0x03) != 0x03)
							System.Threading.Thread.Sleep(100);

						// 気圧データを読み取る (24Bit)
						UInt32 byte0 = piI2c.GetRegister8(0x28);
						UInt32 byte1 = piI2c.GetRegister8(0x29);
						UInt32 byte2 = piI2c.GetRegister8(0x2A);

						// 読み取った値をhPa値に変換する
						Int32 result24 = (Int32)((byte2 << 16) | (byte1 << 8) | byte0);
						Double hPa = (Double)result24 / 4096.0;

						// 温度データを読み取る (16Bit)
						byte0 = piI2c.GetRegister8(0x2B);
						byte1 = piI2c.GetRegister8(0x2C);

						// 読み取った値を℃に変換する
						Int16 result16 = (Int16)((byte1 << 8) | byte0);
						Double temp = 42.5 + (Double)result16 / 480;

						// 測定値を表示する
						Console.WriteLine(String.Format("Pressure = {0:##.0} hPa, Temperature = {1:#.0} °C", hPa, temp));

                        // 1秒間待つ
						System.Threading.Thread.Sleep(500);
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message + " - " + ex.StackTrace);
			}
		}
    }
}
