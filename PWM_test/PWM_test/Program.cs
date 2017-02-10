using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHI.Pins;
using System.Threading;

namespace PWM_test
{
    public class Program
    {
        public static void Main()
        {
            //OutputPort dir = new OutputPort(FEZSpider.Socket8.Pwm7, true);
            double frequence = 100000; // Période en microseconde
            double rapportCyclique = 0.50; // Période en microseconde
            PWM motorDriver = new PWM(FEZSpider.Socket8.Pwm9, frequence, rapportCyclique, false);
            motorDriver.Start();
        }
    }
}