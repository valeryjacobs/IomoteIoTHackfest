using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        static string connString = ConfigurationManager.ConnectionStrings["IoTHub_Conn"].ToString();
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
        public async Task Put(string id, [FromBody]DesiredProperties patch)
        {
            var input = Newtonsoft.Json.JsonConvert.SerializeObject(patch).Replace("\"", "'");
            var p = @"{'properties': {'desired': " + input + "}"; //{ "send_time":"10","log_time":"5","power_mode":"0","digital_in_mode":"0"}
            var twin = await registryManager.GetTwinAsync(id);
            await registryManager.UpdateTwinAsync(twin.DeviceId, p , twin.ETag);
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

    public class DesiredProperties
    {
        public int send_time { get; set; }
        public int log_time { get; set; }
        public int power_mode { get; set; }

        public int digital_in_mode { get; set; }
    }
}
