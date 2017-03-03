using System;
using Microsoft.SPOT;
using GHI.Pins;
using Microsoft.SPOT.Hardware;

namespace TestMicroSwitch
{
    public class Program
    {
        public static void Main()
        {
            Debug.Print(Resources.GetString(Resources.StringResources.String1));
            InputPort microswitch = new InputPort(FEZSpider.Socket4.Pin3,false,Port.ResistorMode.PullDown);
            while (true)
            {
                Debug.Print(microswitch.Read().ToString());
            }

        }
    }
}
