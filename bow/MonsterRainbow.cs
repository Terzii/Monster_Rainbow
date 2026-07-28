using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

internal static class MonsterRainbow
{
    private const string MutexName = "Local\\MonsterRainbow.SingleInstance";
    private const string StopEventName = "Local\\MonsterRainbow.Stop";

    [DllImport("InsydeDCHU.dll", CallingConvention = CallingConvention.Winapi)]
    private static extern int SetDCHU_Data(int command, byte[] data, int length);

    [DllImport("InsydeDCHU.dll", CallingConvention = CallingConvention.Winapi)]
    private static extern int WriteAppSettings(int group, int key, int length, ref byte data);

    [STAThread]
    private static void Main()
    {
        try
        {
            Run();
        }
        catch (Exception ex)
        {
            try
            {
                string folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MonsterRainbow");
                Directory.CreateDirectory(folder);
                File.AppendAllText(Path.Combine(folder, "hata.log"),
                    DateTime.Now + Environment.NewLine + ex + Environment.NewLine);
            }
            catch { }
        }
    }

    private static void Run()
    {
        bool ownsMutex;
        using (var mutex = new Mutex(true, MutexName, out ownsMutex))
        {
            if (!ownsMutex)
            {
                try { EventWaitHandle.OpenExisting(StopEventName).Set(); }
                catch (WaitHandleCannotBeOpenedException) { }
                return;
            }

            using (var stop = new EventWaitHandle(false, EventResetMode.AutoReset, StopEventName))
            {
                TurnOn();
                byte hue = 0;

                // Yaklasik 5 saniyede bir tam tur: degisim net gorunur ve CPU dusuk kalir.
                while (!stop.WaitOne(80))
                {
                    byte r, g, b;
                    Wheel(hue, out r, out g, out b);
                    SetColor(r, g, b);
                    unchecked { hue += 4; }
                }
            }
        }
    }

    private static void SetColor(byte r, byte g, byte b)
    {
        SetDCHU_Data(103, new byte[] { g, r, b, 240 }, 4);

        // Yerel fonksiyon uzunluk=3 oldugunda isaretciden uc bayt okur.
        // Bu nedenle uc renk de ayni bitisik tamponda bulunmalidir.
        byte[] rgb = new byte[] { r, g, b };
        WriteAppSettings(2, 81, rgb.Length, ref rgb[0]);

        byte mode = 8;
        WriteAppSettings(2, 32, 1, ref mode);
    }

    private static void TurnOn()
    {
        SetDCHU_Data(103, new byte[] { 0, 0, 1, 224 }, 4);
        byte enabled = 1;
        WriteAppSettings(2, 84, 1, ref enabled);
    }

    private static void Wheel(byte position, out byte r, out byte g, out byte b)
    {
        // Tam sayı hesabı kullanır; kayan nokta/Color nesnesi üretmez.
        int p = position;
        if (p < 85)
        {
            r = (byte)(255 - p * 3);
            g = (byte)(p * 3);
            b = 0;
        }
        else if (p < 170)
        {
            p -= 85;
            r = 0;
            g = (byte)(255 - p * 3);
            b = (byte)(p * 3);
        }
        else
        {
            p -= 170;
            r = (byte)(p * 3);
            g = 0;
            b = (byte)(255 - p * 3);
        }
    }
}
