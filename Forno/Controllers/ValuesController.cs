using System.Collections.Generic;
using System.Web.Http;

namespace Forno.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "sone vuote", "puoi cercare altrove" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "vabbo non l'hai capito..";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
            return;
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
            return;
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
            return;
        }
    }
}
