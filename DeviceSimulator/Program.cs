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
        static string iotHubUri = "vjiomote.azure-devices.net";
        static string deviceKey = "/etc4JZ7FBgP0DgJrB4tNmql9JLQap1mo2TmtHZEe2U=";
        static string deviceId = "Device00000001";

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
                    deviceId = "SimulatedDevice",
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
