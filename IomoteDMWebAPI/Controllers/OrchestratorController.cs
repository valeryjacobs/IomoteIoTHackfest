using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.ServiceBus.Messaging;
using System.Threading;

namespace IomoteDMWebAPI.Controllers
{
    public class OrchestratorController : ApiController
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["IoTHub_Conn"].ToString();
        // GET: api/Orchestrator
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/Orchestrator/5
        public async Task<string> Get(string id)
        {
            if (id == "LetturaSensore")
            {
                var serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
                //var commandMessage = new Microsoft.Azure.Devices.Message(Encoding.ASCII.GetBytes(id));

                var methodInvocation = new CloudToDeviceMethod("MeasureTemperature") { ResponseTimeout = TimeSpan.FromSeconds(30) };
                methodInvocation.SetPayloadJson("'Kitchen'");

                var response = await serviceClient.InvokeDeviceMethodAsync("VJHackfestDemo", methodInvocation);

                return response.GetPayloadAsJson();
            }

            return "refused";
            //EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, "messages/events");
            //var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;
            //CancellationTokenSource cts = new CancellationTokenSource();

            //await serviceClient.SendAsync("myFirstDevice", commandMessage);
            //return "handling the '" + id + "' command.";
        }

        //// POST: api/Orchestrator
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Orchestrator/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Orchestrator/5
        //public void Delete(int id)
        //{
        //}
    }
}
