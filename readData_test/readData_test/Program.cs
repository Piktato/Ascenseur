using System;
using Microsoft.SPOT;
using System.Text;
using System.Threading;
using System.IO.Ports;

namespace readData_test
{
    public class Program
    {
        public static void Main()
        {
            SerialPort UART = new SerialPort("COM3", 115200);
            byte[] rx_byte = new byte[1];
            UART.Open();

            while (true)
            {
                // Lecture du byte              
                if (UART.Read(rx_byte, 0, 1) > 0)// do we have data?  
                {
                    string counter_string = rx_byte[0].ToString();
                    // Converti la chaine en bytes                
                    byte[] buffer = Encoding.UTF8.GetBytes(counter_string);
                    string output = string.Empty;

                    foreach (char item in UTF8Encoding.UTF8.GetChars(buffer))
                    {
                        output += item;
                    }

                    switch (output)
                    {
                        case "49":
                            output = "1";
                            break;
                        case "50":
                            output = "2";
                            break;
                        case "51":
                            output = "3";
                            break;
                        case "115":
                            output = "stop";
                            break;
                    }

                    Debug.Print(output);
                }
            }
        }
    }
}
