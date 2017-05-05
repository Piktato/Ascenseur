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
        static int etageToGo;

        //  Communication
        static SerialPort UART = new SerialPort("COM3", 115200);
        static byte[] rx_byte = new byte[1];

        //  Capteurs
        static AnalogInput captBas = new AnalogInput(Cpu.AnalogChannel.ANALOG_0);
        static AnalogInput captHaut = new AnalogInput(Cpu.AnalogChannel.ANALOG_3);

        //  Microswitch
        static InputPort microSwitchBas = new InterruptPort(FEZSpider.Socket4.Pin3, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeLow);
        static InputPort microSwitchHaut = new InterruptPort(FEZSpider.Socket3.Pin3, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeLow);

        //  Motor
        static OutputPort dir = new OutputPort(FEZSpider.Socket11.Pin9, true);
        static PWM motorDriver;

        static bool arrive = false;

        static Thread goEtage = new Thread(aLetage);

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

        static int calculateDistance()
        {
            int moyenne = 0;

            for (int i = 0; i < 100; i++)
            {
                moyenne += captBas.ReadRaw();
            }

            moyenne /= 100;

            return moyenne;
        }

        public static void aLetage()
        {
            int oldMoyenne = 0;
            changerEtatMotor(true);
            UpOrDown(calculateDistance());

            while (!arrive)
            {
                int moyenne = calculateDistance();

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
                arrive = arriverALetage(oldMoyenne);
            }

            changerEtatMotor(false);
            goEtage.Suspend();
        }

        static bool arriverALetage(int distance)
        {
            bool stop = false;
            int[] distEtage = etages[etageToGo] as int[];

            if (distance < distEtage[1] && distance > distEtage[0])
            {
                /*
                Debug.Print(distEtage[0].ToString());
                Debug.Print(distEtage[1].ToString());
                Debug.Print(distance.ToString());
                */
                Debug.Print("Arriver");
                stop = true;
            }

            return stop;
        }

        static void UpOrDown(int state)
        {
            int[] etage = etages[etageToGo] as int[];
            if (state < etage[1] && state < etage[0])
            {
                Debug.Print("Trop Haut");
                changeSensMotor(false);
            }
            else
            {
                Debug.Print("Trop Bas");
                changeSensMotor(true);
            }
        }

        static void microswitch_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (data1 == 33)
            {
                Debug.Print("Clique Monte");
                changeSensMotor(false);
            }
            if (data1 == 1)
            {
                Debug.Print("Clique Descend");
                changeSensMotor(true);
            }
        }

        static void changeSensMotor(bool sens)
        {
            Debug.Print("Inverse");
            dir.Write(sens);
        }

        static void changerEtatMotor(bool etat)
        {
            if (etat)
            {
                motorDriver.Start();
                Debug.Print("Start");
            }
            else
            {
                Debug.Print("Stop");
                motorDriver.Stop();
            }
        }

        static void UART_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            UART.Read(rx_byte, 0, 1);
            char car = Convert.ToChar(rx_byte[0]);
            if (car != 's')
            {
                Debug.Print("Etage" + car);
                etageToGo = Convert.ToInt32(car.ToString()) - 1;
                arrive = false;
                //if (goEtage.ThreadState)
                //{
                    Debug.Print("Start Thread " + goEtage.ThreadState.ToString());
                    goEtage.Start();
                //}
            }
            else
            {
                Debug.Print("Stop command");
                changerEtatMotor(false);
            }
        }
    }
}
