using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHI.Pins;
using System.Threading;
using System.Text;
using System.IO.Ports;

namespace ascenseur_prog
{
    public class Program
    {
        public static void Main()
        {
            AnalogInput captBas = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);
            AnalogInput captHaut = new AnalogInput(Cpu.AnalogChannel.ANALOG_3);
            OutputPort dir = new OutputPort(FEZSpider.Socket8.Pin9, true);
            InputPort microswitchBas = new InputPort(FEZSpider.Socket4.Pin3, true, Port.ResistorMode.PullDown);
            InputPort microSwitchHaut = new InputPort(FEZSpider.Socket3.Pin3, true, Port.ResistorMode.PullDown);

            double frequence = 38000; //Période en microseconde
            double rapportCyclique = 0.5; // Période en microseconde

            PWM motorDriver = new PWM(FEZSpider.Socket8.Pwm7, frequence, rapportCyclique, false);

            motorDriver.Start();
            int i = 0;
            while (true)
            {
                if (microswitchBas.Read() && dir.Read())
                {
                    dir.Write(false);
                    Debug.Print("Monte");
                }

                if (microSwitchHaut.Read() && !dir.Read())
                {
                    Debug.Print("Descend");
                    dir.Write(true);
                }

                if (microswitchBas.Read() && microSwitchHaut.Read())
                {
                    motorDriver.Stop();
                    return;
                }

                i++;
                if (i > 750)
                {
                    double distHaut = 100 * captHaut.Read();
                    double distBas = 100 * captBas.Read();
                    i = 0;
                }
            }
        }

        public void recieveData()
        {
            
            
        }
    }
}
