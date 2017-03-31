using System;
using Microsoft.SPOT;
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;
using System.Threading;
using GHI.Pins;
using GHI.Processor;
using System.IO.Ports;
using System.Text;
namespace command_asc
{
    public class Program
    {
        public static Window windows;

        public static SerialPort UART = new SerialPort("COM3", 115200);

        public static void Main()
        {
            UART.Open();

            windows = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.windows));

            GlideTouch.Initialize();

            for (int i = 1; i <= 3; i++)
            {
                Button btn = (Button)windows.GetChildByName(i.ToString());
                btn.TapEvent += OnTap;
            }

            Button stopBtn = (Button)windows.GetChildByName("stop");
            stopBtn.TapEvent += stop;

            Glide.MainWindow = windows;
            Thread.Sleep(-1);
        }

        private static void sendCommand(string command)
        {
            byte[] dataSend = Encoding.UTF8.GetBytes(command);
            UART.Write(dataSend, 0, dataSend.Length);
            Debug.Print("Send !");
        }

        private static void OnTap(object sender)
        {
            Debug.Print("Etage : " + (sender as Button).Name);
            sendCommand((sender as Button).Name);
        }

        private static void stop(object sender)
        {
            Debug.Print("Stop");
            sendCommand("s");
        }
    }
}