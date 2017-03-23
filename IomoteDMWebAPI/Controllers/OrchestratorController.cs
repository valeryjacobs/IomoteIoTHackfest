using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IomoteDMWebAPI.Controllers
{
    public class OrchestratorController : ApiController
    {
        // GET: api/Orchestrator
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/Orchestrator/5
        public string Get(string id)
        {
            return "handling the '" + id + "' command.";
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
