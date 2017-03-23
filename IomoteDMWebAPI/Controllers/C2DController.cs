using Microsoft.Azure.Devices;
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
    public class C2DController : ApiController
    {
        static ServiceClient serviceClient;
        static string connectionString = ConfigurationManager.ConnectionStrings["IoTHub_Conn"].ToString();


        public async Task<string> Get()
        {
            return "test";
        }

        public async Task Post([FromBody]string value)
        {
           await  InvokeMethod(value, "test", "testp");
        }

        private static async Task InvokeMethod(string deviceId, string methodName, string payload)
        {
            var methodInvocation = new CloudToDeviceMethod(methodName) { ResponseTimeout = TimeSpan.FromSeconds(30) };
            methodInvocation.SetPayloadJson(payload);

            var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
        }

        public async Task Post()
        {
            ServiceClient serviceClient;

            string connectionString = "{iot hub connection string}";


        }

    }
}
