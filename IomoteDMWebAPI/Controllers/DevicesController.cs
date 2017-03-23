using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IomoteDMWebAPI.Controllers
{
    public class DevicesController : ApiController
    {
        static RegistryManager registryManager;
        static string connString = "HostName=iomoteHub01.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ngON3nUi7bSYvkYOU WHISHDlOVm2GiQ589FN8EpA=";// "HostName=vjiomote.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4tFS5o6pC6fYOU WHISHHu5o/kticwRh+M=";
        static ServiceClient client;
        static JobClient jobClient;

        // GET: api/Devices
        public async Task<IEnumerable<Device>> Get()
        {
            return await GetDevices();
        }

        // GET: api/Devices/5
        public async Task<Twin> Get(string id)
        {
            return await RegistryManager.CreateFromConnectionString(connString).GetTwinAsync(id);
        }

        // POST: api/Devices
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Devices/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Devices/5
        public void Delete(int id)
        {
        }

        private async Task<IEnumerable<Device>> GetDevices()
        {
            IEnumerable<Device> iotHubDevices;
            List<Device> devicesDictionary = new List<Device>();

            registryManager = RegistryManager.CreateFromConnectionString(connString);

            // var x = await registryManager.GetDeviceAsync("Device00000001");
            iotHubDevices = await registryManager.GetDevicesAsync(1000000);

            foreach (var device in iotHubDevices)
            {
                devicesDictionary.Add(device);
            }


            return devicesDictionary;
        }
    }
}
