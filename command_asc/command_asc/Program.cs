/*
 *  Auteur : Paschoud Nicolas, Ramushi Ardi
 *  Projet : command_asc
 *  Description : Envoie les commandes de l'ascenseur d'une carte à l'autre
 *  Date : 27 janvier 2017
 *  Version : 1.0
 */

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

        //  Port de communication avec l'autre carte
        public static SerialPort UART = new SerialPort("COM3", 115200);
        
        public static void Main()
        {
            UART.Open();

            //  Création de l'interface graphique sur l'écran
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
            Thread.Sleep(Timeout.Infinite);
        }

        // Envois les données à l'autre carte
        private static void sendCommand(string command)
        {
            byte[] dataSend = Encoding.UTF8.GetBytes(command);
            UART.Write(dataSend, 0, dataSend.Length);
            Debug.Print("Send !");
        }

        // Evénement clique sur les boutton d'étage
        private static void OnTap(object sender)
        {
            Debug.Print("Etage : " + (sender as Button).Name);
            sendCommand((sender as Button).Name);
        }

        // Evénement clique sur le bouton stop
        private static void stop(object sender)
        {
            Debug.Print("Stop");
            sendCommand("s");
        }
    }
}