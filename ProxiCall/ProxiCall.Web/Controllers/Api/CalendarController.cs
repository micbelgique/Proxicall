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

        [HttpGet("events/{userEmailAddress}")]
        public async Task<IActionResult> GetEventsOfTheDay(string userEmailAddress)
        {
            var startTime = DateTime.Now.Date + new TimeSpan(0, 0, 0);
            var endTime = DateTime.Now.Date + new TimeSpan(23, 59, 59);

            var result = await _msGraphClient.GetEventsOfUser(userEmailAddress, startTime, endTime);
            return Ok(result);
        }
    }
}