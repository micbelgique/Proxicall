using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProxiCall.Web.Services.MsGraph;

namespace ProxiCall.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly MsGraphClient _msGraphClient;

        public CalendarController(MsGraphClient msGraphClient)
        {
            _msGraphClient = msGraphClient;
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents()
        {
            var result = await _msGraphClient.GetEvents();
            return Ok(result);
        }
    }
}