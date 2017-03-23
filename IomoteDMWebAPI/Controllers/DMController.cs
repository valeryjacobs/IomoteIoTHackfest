using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IomoteDMWebAPI.Controllers
{
    public class DMController : ApiController
    {
        static RegistryManager registryManager;
        static string connString = "HostName=vjiomote.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4tFS5o6pC6YOU WHISH Hu5o/kticwRh+M=";
        static ServiceClient client;
        static JobClient jobClient;
        // GET: api/DM
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/DM/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/DM
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/DM/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/DM/5
        public void Delete(int id)
        {
        }
    }
}
