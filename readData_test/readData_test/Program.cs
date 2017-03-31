using System;
using Microsoft.SPOT;
using System.Text;
using System.IO.Ports;

namespace readData_test
{
    public class Program
    {
        public static void Main()
        {

            SerialPort UART = new SerialPort("COM1", 115200);
            int read_count = 0;
            byte[] rx_byte = new byte[1];
            UART.Open();
            read_count = UART.Read(rx_byte, 0, 1);

            for (int i = 0; i < read_count; i++)
            {
                Debug.Print(UART.Read(rx_byte, 0, 1).ToString());
            }
        }
    }
}
