using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuraServiceLib;

namespace csharp_implementation_aura
{
    class WrappedDevice {
        private string name;
        private IAuraSyncDevice device;
        private bool busy = false;
        public WrappedDevice(IAuraSyncDevice thisDevice)
        {
            device = thisDevice;
            name = thisDevice.Name;
        }

        public void setLights(int r, int g, int b) {
            Console.WriteLine($"{name}: Called setLights");
            if (!busy)
            {
                busy = true;
                Task.Run(() =>
                {
                    Console.WriteLine($"{name}: Setting colors");
                    foreach (IAuraRgbLight light in device.Lights)
                    {
                        light.Red = (byte)r;
                        light.Green = (byte)g;
                        light.Blue = (byte)b;
                    }
                    Console.WriteLine($"{name}: Starting applying");
                    device.Apply();
                    Console.WriteLine($"{name}: Done");
                    busy = false;
                });
            } else
            {
                Console.WriteLine($"{name}: Return because is busy");
            }

        }
    }

    class Program
    {
        public static List<WrappedDevice> wrappedDevices = new List<WrappedDevice>();
        public static int red = 0;
        public static int green = 255;
        public static int blue = 0;

        static void Main(string[] args)
        {
            IAuraSdk sdk = new AuraSdk();
            sdk.SwitchMode();
            foreach (IAuraSyncDevice dev in sdk.Enumerate(0))
            {
                wrappedDevices.Add(new WrappedDevice(dev));
            }

            TimerCallback tmCallback = runUpdate;
            Timer timer = new Timer(tmCallback, "", 0, 33);

            Thread.Sleep(30000);
            timer.Dispose();
        }

        static void runUpdate(object objectInfo)
        {
            green = green - 10;
            blue = blue + 10;
            foreach (var device in wrappedDevices)
            {
                device.setLights(red, green, blue);
            }
        }
    }
}
