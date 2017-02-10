using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHI.Pins;
using System.Threading;

namespace asc_capteur
{
    public class Program
    {
        public static void Main()
        {
            AnalogInput capt = new AnalogInput((Cpu.AnalogChannel)Cpu.AnalogChannel.ANALOG_0);

            while (true)
            {
                Debug.Print("Distance : " + capt.Read().ToString());
                Thread.Sleep(250);
            }
        }
    }
}