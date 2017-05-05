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
        static SerialPort UART = new SerialPort("COM3", 115200);
        static byte[] rx_byte = new byte[1];
        static AnalogInput captBas = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);
        static AnalogInput captHaut = new AnalogInput(Cpu.AnalogChannel.ANALOG_3);
        static OutputPort dir = new OutputPort(FEZSpider.Socket11.Pin9, true);
        static InputPort microSwitchBas = new InterruptPort(FEZSpider.Socket4.Pin3, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeLow);
        static InputPort microSwitchHaut = new InterruptPort(FEZSpider.Socket3.Pin3, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeLow);

        public static void Main()
        {
            UART.DataReceived += UART_DataReceived;
            UART.Open();
                        double frequence = 38000; //Période en microseconde
            double rapportCyclique = 0.5; // Période en microseconde

            microSwitchBas.OnInterrupt += microswitch_OnInterrupt;
            microSwitchHaut.OnInterrupt += microswitch_OnInterrupt;

            PWM motorDriver = new PWM(FEZSpider.Socket11.Pwm7, frequence, rapportCyclique, false);

            motorDriver.Start();
        }

        static void microswitch_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (data1 == 33)
            {
                dir.Write(false);
            }
            if (data1 == 1)
            {
                dir.Write(true);
                
            }
        }

        static void UART_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            UART.Read(rx_byte, 0, 1);

            char car = Convert.ToChar(rx_byte[0]);

            Debug.Print(car.ToString());
        }
    }
}
