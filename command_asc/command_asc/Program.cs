using System;
using Microsoft.SPOT;
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;
using System.Threading;
using GHI.Pins;
using GHI.Processor;
using GTM = Gadgeteer.Modules;

namespace command_asc
{
    public class Program
    {
        public static Window windows;
        public static TextBlock txtttttttt;
        
        public static void Main()
        {
            windows = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.windows));

            GlideTouch.Initialize();

            txtttttttt = (TextBlock)windows.GetChildByName("textBlock");
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

        private static void OnTap(object sender)
        {
            txtttttttt.Text = "";
            txtttttttt.Text = (sender as Button).Name;
            txtttttttt.Invalidate();
        }

        private static void stop(object sender)
        {
            txtttttttt.Text = (sender as Button).Name;
        }
    }
}