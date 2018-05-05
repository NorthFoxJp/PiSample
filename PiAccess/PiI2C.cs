using System;
using System.Threading.Tasks;
using RPi = BCM2835.BCM2835Managed;

namespace PiAccessLib
{
	/// <summary>
    /// Raspberry Pi - I2Cクラス
    /// </summary>
	public class PiI2C : IDisposable
    {
		/// <summary>
        /// I2C使用中
        /// </summary>
        /// <value><c>true</c> if is used; otherwise, <c>false</c>.</value>
        static public Boolean IsUsed { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:PiAccessLib.PiI2C"/> high speed core.
        /// </summary>
        /// <value><c>true</c> if high speed core; otherwise, <c>false</c>.</value>
        public Boolean HighSpeedCore { get; private set; } = true;

        /// <summary>
        /// Gets the frequency.
        /// </summary>
        /// <value>The frequency.</value>
        public UInt32 Frequency { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="desiredFrequency">Desired frequency.</param>
        /// <param name="slaveAddress">Slave address.</param>
        public PiI2C(UInt32 desiredFrequency, byte slaveAddress)
        {
            if (IsUsed == true)
                throw new InvalidOperationException("I2C is in used.");

            RPi.bcm2835_i2c_begin(false, HighSpeedCore);
            Frequency = RPi.bcm2835_i2c_set_baudrate(desiredFrequency, HighSpeedCore);
            RPi.bcm2835_i2c_setSlaveAddress(slaveAddress);
        }

        /// <summary>
        /// 廃棄
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:PiAccessLib.PiI2C"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="T:PiAccessLib.PiI2C"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="T:PiAccessLib.PiI2C"/> so
        /// the garbage collector can reclaim the memory that the <see cref="T:PiAccessLib.PiI2C"/> was occupying.</remarks>
        public void Dispose()
        {
            if (IsUsed == false)
                return;

			RPi.bcm2835_i2c_end();

            IsUsed = false;
        }

        /// <summary>
        /// 書き込み
        /// </summary>
        /// <param name="buffer">書き込むデータのバッファ</param>
        /// <param name="offset">オフセット(インデックス)</param>
        public void Write(Byte[] buffer, Int32 offset)
        {
			ArraySegment<byte> segment = new ArraySegment<byte>(buffer, offset, buffer.Length);
            RPi.bcm2835_i2c_write(segment);
        }

        /// <summary>
        /// 読み込み
        /// </summary>
        /// <returns>The read.</returns>
        /// <param name="buffer">読み取りデータの格納バッファ</param>
		/// <param name="offset">オフセット(インデックス)</param>
        /// <param name="count">読み取りデータバイト数</param>
		/// <returns>読み取りバイト数</returns>
        public Int32 Read(byte[] buffer, Int32 offset, Int32 count)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer, offset, count);
            RPi.bcm2835_i2c_read(segment);
            return count;
        }

        /// <summary>
		/// 汎用:レジスタ読み込み
        /// </summary>
        /// <param name="registerAddress">レジスタ アドレス</param>
        /// <param name="buffer">読み取りデータの格納バッファ</param>
		/// <param name="offset">オフセット(インデックス)</param>
		/// <param name="count">読み取りデータバイト数</param>
        public void ReadRegister(byte registerAddress, byte[] buffer, int offset, int count)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer, offset, count);
            RPi.bcm2835_i2c_read_register_rs(registerAddress, segment);
        }

        /// <summary>
		/// 汎用:レジイスタ書き込み
        /// </summary>
        /// <param name="registerAddress">レジスタ アドレス</param>
		/// <param name="buffer">書き込むデータのバッファ</param>
        /// <param name="offset">オフセット(インデックス)</param>
        public void WriteRegister(byte registerAddress, byte[] buffer, int offset)
        {
			byte[] data = new byte[buffer.Length + 1];
            data[0] = registerAddress;
			Buffer.BlockCopy(buffer, offset, data, 1, buffer.Length);
            RPi.bcm2835_i2c_write(data);
        }

		#region デバイスからの読み込み(非同期)
        /// <summary>
        /// デバイスからの読み込み(非同期)
        /// </summary>
        /// <param name="readLength">読み取りデータ長</param>
        /// <returns>タスク</returns>
        public Task<Byte[]> ReadAsync(Int32 readLength)
        {
            return Task.Run(() =>
            {
                Byte[] buffer = new byte[readLength];
                Read(buffer, 0, buffer.Length);
                return buffer;
            });
        }
        #endregion

        #region デバイスへの書き込み(非同期)
        /// <summary>
        /// デバイスへの書き込み(非同期)
        /// </summary>
        /// <param name="writeData">書き込みデータ</param>
        /// <returns>タスク</returns>
        public Task WriteAsync(byte[] writeData)
        {
            Task task = Task.Run(() =>
            {
                Write(writeData, 0);
            });

            return task;
        }
        #endregion

        #region レジスタからの読み込み(非同期)
        /// <summary>
        /// レジスタからの読み込み(非同期)
        /// </summary>
        /// <param name="registerAddress">レジスタ アドレス</param>
        /// <param name="readByte">読み取りバイト数</param>
        /// <returns>タスク</returns>
        public Task<Byte[]> ReadRegisterAsync(Byte registerAddress, Int32 readByte)
        {
            return Task.Run(() =>
            {
                Byte[] buffer = new byte[readByte];
                ArraySegment<byte> segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
                RPi.bcm2835_i2c_read_register_rs(registerAddress, segment);

                return buffer;
            });
        }
        #endregion

        #region レジスタへの書き込み(非同期)
        /// <summary>
        /// レジスタへの書き込み(非同期)
        /// </summary>
        /// <param name="registerAddress">レジスタ アドレス</param>
        /// <param name="writeData">書き込みデータ</param>
        /// <returns>タスク</returns>
        public Task WriteRegisterAsync(Byte registerAddress, Byte[] writeData)
        {
            return Task.Run(() =>
            {
                byte[] data = new byte[writeData.Length + 1];
                data[0] = registerAddress;

                Buffer.BlockCopy(writeData, 0, data, 1, writeData.Length);
                RPi.bcm2835_i2c_write(data);
            });
        }
        #endregion

        #region レジスタからの読み込み
        /// <summary>
        /// レジスタからの読み込み
        /// </summary>
        /// <param name="registerAddress">レジスタ アドレス</param>
        /// <param name="readByte">読み取りバイト数</param>
        /// <returns>読み込み値</returns>
        private Byte[] ReadRegister(Byte registerAddress, Int32 readByte)
        {
            Byte[] result;
            using (Task<Byte[]> task = ReadRegisterAsync(registerAddress, readByte))
            {
                task.Wait();
                result = task.Result;
            }

            return result;
        }
        #endregion

        #region レジスタへの書き込み
        /// <summary>
        /// レジスタへの書き込み
        /// </summary>
        /// <param name="writeData">書き込みデータ</param>
        private void WriteRegister(Byte registerAddress, Byte[] writeData)
        {
            using (Task task = WriteRegisterAsync(registerAddress, writeData))
            {
                task.Wait();
            }
        }
        #endregion

        #region レジスタ設定(8Bit)
        /// <summary>
        /// レジスタ設定(8Bit)
        /// </summary>
        /// <param name="registerAddress">アドレス</param>
        /// <param name="value">値</param>
        public void SetRegister8(Byte registerAddress, Byte value)
        {
            Byte[] writeData = { value };
            WriteRegister(registerAddress, writeData);
        }
        #endregion

        #region レジスタ設定(16Bit)
        /// <summary>
        /// レジスタ設定(16Bit)
        /// </summary>
        /// <param name="registerAddress">アドレス</param>
        /// <param name="value">値</param>
        public void SetRegister16(Byte registerAddress, UInt16 value)
        {
            Byte[] writeData = { (Byte)((value & 0xff00) >> 8), (Byte)(value & 0x00ff) };
            WriteRegister(registerAddress, writeData);
        }
        #endregion

        #region レジスタ設定(24Bit)
        /// <summary>
        /// レジスタ設定(24Bit)
        /// </summary>
        /// <param name="registerAddress">アドレス</param>
        /// <param name="value">値</param>
        public void SetRegister24(Byte registerAddress, UInt32 value)
        {
            Byte[] writeData = { (Byte)((value & 0x00ff0000) >> 16), (Byte)((value & 0x0000ff00) >> 8), (Byte)(value & 0x00ff) };
            WriteRegister(registerAddress, writeData);
        }
        #endregion

        #region レジスタ設定(32Bit)
        /// <summary>
        /// レジスタ設定(32Bit)
        /// </summary>
        /// <param name="registerAddress">アドレス</param>
        /// <param name="value">値</param>
        public void SetRegister32(Byte registerAddress, UInt32 value)
        {
            Byte[] writeData = { (Byte)((value & 0xff000000) >> 24), (Byte)((value & 0x00ff0000) >> 16), (Byte)((value & 0x0000ff00) >> 8), (Byte)(value & 0x00ff) };
            WriteRegister(registerAddress, writeData);
        }
        #endregion

        #region レジスタ取得(8Bit)
        /// <summary>
        /// レジスタ取得(8Bit)
        /// </summary>
        /// <param name="registerAddress">アドレス</param>
        /// <returns>レジスタ値</returns>
        public Byte GetRegister8(Byte registerAddress)
        {
            Byte[] readData = ReadRegister(registerAddress, 1);

            return readData[0];
        }
        #endregion

        #region レジスタ取得(16Bit)
        /// <summary>
        /// レジスタ取得(16Bit)
        /// </summary>
        /// <param name="registerAddress">アドレス</param>
        /// <returns>レジスタ値</returns>
        public UInt16 GetRegister16(Byte registerAddress)
        {
            Byte[] readData = ReadRegister(registerAddress, 2);

            return (UInt16)((UInt16)readData[0] << 8 | (UInt16)readData[1]);
        }
        #endregion

        #region レジスタ取得(24Bit)
        /// <summary>
        /// レジスタ取得(24Bit)
        /// </summary>
        /// <param name="registerAddress">アドレス</param>
        /// <returns>レジスタ値</returns>
        public UInt32 GetRegister24(Byte registerAddress)
        {
            Byte[] readData = ReadRegister(registerAddress, 3);

            return ((UInt32)readData[0] << 16) | ((UInt32)readData[1] << 8) | (UInt32)readData[2];
        }
        #endregion

        #region レジスタ取得(32Bit)
        /// <summary>
        /// レジスタ取得(32Bit)
        /// </summary>
        /// <param name="registerAddress">アドレス</param>
        /// <returns>レジスタ値</returns>
        public UInt32 GetRegister32(Byte registerAddress)
        {
            Byte[] readData = ReadRegister(registerAddress, 4);

            return ((UInt32)readData[0] << 24) | ((UInt32)readData[1] << 16) | ((UInt32)readData[2] << 8) | (UInt32)readData[3];
        }
        #endregion
    }
}
