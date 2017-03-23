using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManager
{
    class Program
    {
        static RegistryManager registryManager;
        static string connString = "HostName=vjiomote.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4tFS5o6pC6ff0bol57YOU WHISHo/kticwRh+M=";

        static void Main(string[] args)
        {

            var x = Start();

            Console.ReadLine();
        }

        public static async Task<IEnumerable<Device>> Start()
        {
            IEnumerable<Device> iotHubDevices;
            List<Device> devicesDictionary = new List<Device>();

            registryManager = RegistryManager.CreateFromConnectionString(connString);

            // var x = await registryManager.GetDeviceAsync("Device00000001");
            iotHubDevices = await registryManager.GetDevicesAsync(1000);

            foreach (var device in iotHubDevices)
            {
                devicesDictionary.Add(device);
            }


            return devicesDictionary;
        }


    }
}
