using System;
using BCM2835;

namespace PiAccessLib
{
	/// <summary>
	/// BCM2835基本クラス
    /// </summary>
	public class BCM2835Basic : IDisposable
	{
		/// <summary>
        /// コンストラクタ
        /// </summary>
		public BCM2835Basic()
		{
			BCM2835Managed.bcm2835_init();
		}
	    
        /// <summary>
        /// 廃棄
        /// </summary>
		public void Dispose()
		{
			BCM2835Managed.bcm2835_close();
		}
    }
}
