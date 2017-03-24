using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceSimulator
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "iomoteHub01.azure-devices.net";
        static string deviceKey = "rReAqYjrLxC2BDXT2ZY32DBxmvKfxBoVYB8Ja6I6dKM=";
        static string deviceId = "VJHackfestDemo";

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));

            StartD2CAsync();

            Console.ReadLine();
        }

        private static async void StartD2CAsync()
        {
            double avgTemp = 19;
            Random rand = new Random();

            while (true)
            {
                double currentTemp = avgTemp + rand.NextDouble() * 4 - 2;

                var telemetryDataPoint = new
                {
                    room = "Kitchen",
                    currentTemp = currentTemp
                };

                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                Task.Delay(1000).Wait();
            }
        }
    }
}
