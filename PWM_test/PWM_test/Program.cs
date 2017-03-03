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
            //OutputPort dir = new OutputPort(FEZSpider.Socket8.Pin9, true);
            double frequence = 38000; // Période en microseconde
            double rapportCyclique = 0.5; // Période en microseconde
            PWM motorDriver = new PWM(FEZSpider.Socket8.Pwm7, frequence, rapportCyclique, false);
            motorDriver.Stop();
            int i = 0;
            /*while (true)
            {
                i++;
                if (i >= 100000)
                {
                    dir.Write(!dir.Read());
                    i = 0;
                }
            }*/
        }
    }
}