# PiSample
Raspberry Pi – Mono C# Sample Program

このサンプルコードは、Mono C# + BCM2835Managed　で作成されています。<br/>
<br/>
<q>
BCM2835Managedは、<a href="https://github.com/gusmanb"> Agustín Gimenez Bernadさん</a>の<a href="https://github.com/gusmanb/BCM2835Managed">https://github.com/gusmanb/BCM2835Managed</a> です。
</q>
<br/>
<br/>
各プロジェクトには、以下のものが入っています。<br/>
<ul>
<li>BCM2835Managed --- <a href="https://github.com/gusmanb/BCM2835Managed">BCM2835Managed</a>のプロジェクトです。</li>
<li>PiAccessLib ---　BCM2835Managedを使用したI2Cのクラスが入っています。</li>
<li>SampleAppMain　---　Visual Studio 2017 for Mac版とWindows版で共通のメイン部分です。</li>
<li>Sample4MacDev ---　Visual Studio 2017 for Mac用のプロジェクトです。</li>
</ul>

以下のように、I2Cデバイスとのやり取りを行います。<br/>
※ <a href="https://media.digikey.com/pdf/Data%20Sheets/ST%20Microelectronics%20PDFS/LPS331AP.pdf">LPS331AP</a> 気圧センサを使用しています。<br/>

<pre>
<code>
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

        // 500msec待つ
        System.Threading.Thread.Sleep(500);
    }
}
</code>
</pre>
