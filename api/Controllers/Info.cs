using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        // GET api/
        [HttpGet]
        public string Get()
        {
            return "Hello from the api";
        }
    }
}
