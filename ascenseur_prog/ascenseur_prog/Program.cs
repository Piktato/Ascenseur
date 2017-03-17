using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHI.Pins;
using System.Threading;

namespace ascenseur_prog
{
    public class Program
    {
        public static void Main()
        {
            AnalogInput captBas = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);
            AnalogInput captHaut = new AnalogInput(Cpu.AnalogChannel.ANALOG_3);
            OutputPort dir = new OutputPort(FEZSpider.Socket8.Pin9, true);
            InputPort microswitch = new InputPort(FEZSpider.Socket4.Pin3, true, Port.ResistorMode.PullDown);
            InputPort microSwitch2 = new InputPort(FEZSpider.Socket3.Pin3, true, Port.ResistorMode.PullDown);

            double frequence = 20000; // Période en microseconde
            double rapportCyclique = 0.5; // Période en microseconde

            PWM motorDriver = new PWM(FEZSpider.Socket8.Pwm7, frequence, rapportCyclique, false);

            motorDriver.Start();

            while (true)
            {
                if (microswitch.Read())
                {
                    dir.Write(false);
                    Debug.Print("Monte");
                }

                if (microSwitch2.Read())
                {
                    Debug.Print("Descend");
                    dir.Write(true);
                }
            
                double dist = 100 * captHaut.Read();
                Debug.Print("Distance : " + dist.ToString());
            }
        }
    }
}
