using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHI.Pins;
using System.Threading;
using System.Text;
using System.IO.Ports;
using System.Collections;
namespace ascenseur_prog
{
    public class Program
    {
        static ArrayList etages = new ArrayList();

        static int[] etage1 = { 140, 150 };
        static int[] etage2 = { 280, 290 };
        static int[] etage3 = { 480, 490 };

        //  Communication
        static SerialPort UART = new SerialPort("COM3", 115200);
        static byte[] rx_byte = new byte[1];
        //  Capteur
        static AnalogInput captBas = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);
        static AnalogInput captHaut = new AnalogInput(Cpu.AnalogChannel.ANALOG_3);

        //  Microswitch
        static InputPort microSwitchBas = new InterruptPort(FEZSpider.Socket4.Pin3, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeLow);
        static InputPort microSwitchHaut = new InterruptPort(FEZSpider.Socket3.Pin3, false, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeLow);

        //  Motor
        static OutputPort dir = new OutputPort(FEZSpider.Socket11.Pin9, true);
        static PWM motorDriver;

        static bool arrive = false;
        static bool arreter = true;

        public static void Main()
        {
            etages.Add(etage1);
            etages.Add(etage2);
            etages.Add(etage3);

            UART.DataReceived += UART_DataReceived;
            UART.Open();
            double frequence = 38000; //Période en microseconde
            double rapportCyclique = 0.5; // Période en microseconde
            motorDriver = new PWM(FEZSpider.Socket11.Pwm7, frequence, rapportCyclique, true);

            microSwitchHaut.OnInterrupt += microswitch_OnInterrupt;
            microSwitchBas.OnInterrupt += microswitch_OnInterrupt;

            motorDriver.Stop();

            Thread.Sleep(-1);
        }

        static int calculateDistance(AnalogInput capt)
        {
            int moyenne = 0;

            for (int i = 0; i < 100; i++)
            {
                moyenne += captBas.ReadRaw();
            }

            moyenne /= 100;

            return moyenne;
        }

        public static void aLetage(int etageToGo)
        {
            int oldMoyenne = 0;
            changerEtatMotor(!arreter);

            while (!arrive)
            {
                int moyenne = calculateDistance(captBas);

                if (dir.Read())
                {
                    if (moyenne < oldMoyenne)
                    {
                        Debug.Print("Descend : moyenne  " + moyenne);
                        oldMoyenne = moyenne;
                    }
                }
                else
                {
                    if (moyenne > oldMoyenne)
                    {
                        Debug.Print("Monte : moyenne  " + moyenne);
                        oldMoyenne = moyenne;
                    }
                }

                arrive = arriverALetage(oldMoyenne, etageToGo);
            }
            arrive = false;

            changerEtatMotor(!arreter);
        }
        static bool arriverALetage(int distance, int etageToGo)
        {
            bool stop = false;
            int[] distEtage = etages[etageToGo] as int[];

            if (distance < distEtage[1] && distance > distEtage[0])
            {
                Debug.Print(distEtage[0].ToString());
                Debug.Print(distEtage[1].ToString());
                Debug.Print(distance.ToString());

                stop = true;
            }

            return stop;
        }
        static void microswitch_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (data1 == 33)
            {
                changeSensMotor(false);
            }
            if (data1 == 1)
            {
                changeSensMotor(true);
            }
        }


        static void changeSensMotor(bool sens)
        {
            dir.Write(sens);
        }

        static void changerEtatMotor(bool etat)
        {
            if (!etat)
                motorDriver.Start();
            else
                motorDriver.Stop();
            arreter = !arreter;
        }

        static void UART_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            UART.Read(rx_byte, 0, 1);
            char car = Convert.ToChar(rx_byte[0]);
            if (car != 's')
            {
                aLetage(Convert.ToInt32(car.ToString()) - 1);
            }
            else
            {
                changerEtatMotor(!arreter);
            }
        }
    }
}
