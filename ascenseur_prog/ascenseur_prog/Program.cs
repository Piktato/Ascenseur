/*
 * Auteur : Ramushi Ardi & Paschoud Nicolas
 * Nom de l'application : ascenseur_prog
 * Description : Programme pour le fonctionnement d'un ascenseur miniature
 * Version : 1.0
 * Date : 26 janvier 2017
 */
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
        static ArrayList immeuble = new ArrayList();

        //  Constante pour les étages
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

        //  Microswitchs
        static InputPort microSwitchBas = new InterruptPort(FEZSpider.Socket4.Pin3, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);
        static InputPort microSwitchHaut = new InterruptPort(FEZSpider.Socket3.Pin3, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);

        //  Moteur
        static OutputPort sensRotation = new OutputPort(FEZSpider.Socket11.Pin9, true);
        static OutputPort motorDriver = new OutputPort(FEZSpider.Socket11.Pin7, true);

        static bool arrive = false;

        static Thread goEtage = new Thread(aLetage);

        public static void Main()
        {
            immeuble.Add(etage1);
            immeuble.Add(etage2);
            immeuble.Add(etage3);

            UART.DataReceived += UART_DataReceived;
            UART.Open();

            microSwitchHaut.OnInterrupt += microswitch_OnInterrupt;
            microSwitchBas.OnInterrupt += microswitch_OnInterrupt;

            motorDriver.Write(true);

            Thread.Sleep(Timeout.Infinite);
        }

        //  Calcul de la hauteur de l'ascenseur
        static int calculeHauteur()
        {
            int moyenne = 0;

            for (int i = 0; i < 100; i++)
            {
                moyenne += captBas.ReadRaw();
            }

            moyenne /= 100;

            return moyenne;
        }

        //  Fonction de déplacement de l'ascenseur
        //  Cette fonction va gérer l'ascenseur jusqu'à ce qu'il arrive à l'étage demander
        public static void aLetage()
        {
            int oldMoyenne = 0;
            int moyenne = calculeHauteur();

            //  Démarage de l'ascenseur
            UpOrDown(moyenne);
            StopStart(false);
            oldMoyenne = calculeHauteur();

            while (!arrive)
            {
                moyenne = calculeHauteur();
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

        //  Test si l'ascenseur est arrivé
        //  return true si il est arrivé
        static bool arriverALetage(int hauteurAsc)
        {
            bool stop = false;
            int[] distEtage = immeuble[etageToGo] as int[];

            if (hauteurAsc < distEtage[1] && hauteurAsc > distEtage[0])
            {
                Debug.Print("Arriver");
                stop = true;
            }

            return stop;
        }

        //  Fonction pour savoir si il doit 
        //  monter ou descendre pour atteindre l'étage demandé
        static void UpOrDown(int hauteurAsc)
        {
            int[] etage = immeuble[etageToGo] as int[];
            if (hauteurAsc < etage[1] && hauteurAsc < etage[0])
            {
                //  L'ascenseur est trop bas
                //  -> On le fait monter
                changeSensMotor(false);
            }
            else
            {
                //  L'ascenseur est trop haut
                //  -> On le fait descendre
                changeSensMotor(true);
            }
        }

        //  Evénement clique des microswitch
        static void microswitch_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (data1 == 33)
            {
                //  Microswitch du bas
                //  On inverse le sens de rotation pour le faire monter
                sensRotation.Write(false);
            }
            if (data1 == 1)
            {
                //  Microswitch du Haut
                //  On inverse le sens de rotation pour le faire descendre
                sensRotation.Write(true);
            }
        }

        //  Fonction pour changer le sens de rotation du moteur
        //  bool sens : false pour monter true pour descendre
        static void changeSensMotor(bool sens)
        {
            sensRotation.Write(sens);
            StopStart(false);
        }

        //  Fonction pour arrêter/démarrer le moteur
        static void StopStart(bool stop)
        {
            if (stop)
            {
                //  Arrêt du moteur
                motorDriver.Write(sensRotation.Read());
            }
            else
            {
                //  Démarage du moteur
                motorDriver.Write(!sensRotation.Read());
            }
        }
        
        //  Evénement se déclanchant lorsque la carte reçoit des données
        static void UART_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //  Récupération des données envoyée par l'autre carte
            UART.Read(rx_byte, 0, 1);
            char car = Convert.ToChar(rx_byte[0]);
            
            //  Si ce n'est pas un s
            //  C'est qu'on souhaite aller à un étage
            if (car != 's')
            {
                //  Démarrage du Thread pour faire déplacer l'ascenseur au bon endroit 
                Debug.Print("Etage : " + car);
                etageToGo = Convert.ToInt32(car.ToString()) - 1;
                arrive = false;
                goEtage = new Thread(aLetage);
                goEtage.Start();
            }
            else
            {
                //  Sinon, c'est qu'on souhaite l'arrêter
                StopStart(true);
            }
        }
    }
}
