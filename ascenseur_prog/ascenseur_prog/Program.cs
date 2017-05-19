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
        static ArrayList listEtage = new ArrayList();

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
        static InputPort microSwitchBas = new InterruptPort(FEZSpider.Socket4.Pin3, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);
        static InputPort microSwitchHaut = new InterruptPort(FEZSpider.Socket3.Pin3, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);

        //  Motor
        static OutputPort sensRotation = new OutputPort(FEZSpider.Socket11.Pin9, true);
        static OutputPort motorDriver;

        static bool arrive = false;

        static Thread goEtage = new Thread(aLetage);

        public static void Main()
        {
            etages.Add(etage1);
            etages.Add(etage2);
            etages.Add(etage3);

            UART.DataReceived += UART_DataReceived;
            UART.Open();

            motorDriver = new OutputPort(FEZSpider.Socket11.Pin7, true);

            microSwitchHaut.OnInterrupt += microswitch_OnInterrupt;
            microSwitchBas.OnInterrupt += microswitch_OnInterrupt;

            motorDriver.Write(true);

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
            int moyenne = calculateDistance();
            UpOrDown(moyenne);
            StopStart(false);
            oldMoyenne = calculateDistance();

            while (!arrive)
            {
                moyenne = calculateDistance();
                if (moyenne <= oldMoyenne && sensRotation.Read())
                {
                    Debug.Print("Descend : moyenne  " + moyenne);
                    oldMoyenne = moyenne;
                }

                if (moyenne >= oldMoyenne && !sensRotation.Read())
                {
                    Debug.Print("Monte : moyenne  " + moyenne);
                    oldMoyenne = moyenne;
                }
                arrive = arriverALetage(oldMoyenne);
            }

            StopStart(true);
            goEtage.Suspend();
        }

        static bool arriverALetage(int distance)
        {
            bool stop = false;
            int[] distEtage = etages[etageToGo] as int[];

            if (distance < distEtage[1] && distance > distEtage[0])
            {
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
                changeSensMotor(false);
            }
            else
            {
                changeSensMotor(true);
            }
        }

        static void microswitch_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (data1 == 33)
            {
                sensRotation.Write(false);
                motorDriver.Write(true);
            }
            if (data1 == 1)
            {
                sensRotation.Write(true);
                motorDriver.Write(false);
            }
        }

        static void changeSensMotor(bool sens)
        {
            sensRotation.Write(sens);
            StopStart(false);
        }

        static void StopStart(bool stop)
        {
            if (stop)
            {
                motorDriver.Write(sensRotation.Read());
            }
            else
            {
                motorDriver.Write(!sensRotation.Read());
            }
        }

        static void UART_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            UART.Read(rx_byte, 0, 1);
            char car = Convert.ToChar(rx_byte[0]);
            if (car != 's')
            {
                Debug.Print("Etage : " + car);
                etageToGo = Convert.ToInt32(car.ToString()) - 1;
                arrive = false;
                goEtage = new Thread(aLetage);
                goEtage.Start();
            }
            else
            {
                StopStart(true);
            }
        }
    }
}
