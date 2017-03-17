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
            AnalogInput capt = new AnalogInput((Cpu.AnalogChannel)Cpu.AnalogChannel.ANALOG_0);
            OutputPort dir = new OutputPort(FEZSpider.Socket8.Pin9, true);
            InputPort microswitch = new InputPort(FEZSpider.Socket4.Pin3, false, Port.ResistorMode.PullDown);
            
            double frequence = 38000; // Période en microseconde
            double rapportCyclique = 0.5; // Période en microseconde

            PWM motorDriver = new PWM(FEZSpider.Socket8.Pwm7, frequence, rapportCyclique, false);

            motorDriver.Stop();
                

            while (true)
            {
                if (microswitch.Read())
                {
                    motorDriver.Stop();
                }

                Debug.Print("Distance : " + capt.Read().ToString());

                Debug.Print(microswitch.Read().ToString());
                Thread.Sleep(50);
            }

        }
    }
}