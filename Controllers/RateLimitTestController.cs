using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AdvancedRateLimitedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateLimitTestController : ControllerBase
    {
        // GET api/rateLimitTest
        [HttpGet]
        public IActionResult Get()
        {
            // Simulate a test request that will go through the rate-limiting middleware
            return Ok("Request successful");
        }
    }

    [Route("[controller]")]
    [ApiController]

    public class weatherController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Weather API accessed successfully");
        }
    }


    [Route("[controller]")]
    [ApiController]

    public class api1Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API_1 accessed successfully");
        }
    }

    [Route("[controller]")]
    [ApiController]

    public class api2Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API_2 accessed successfully");
        }
    }

    [Route("[controller]")]
    [ApiController]

    public class statusController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Status API accessed successfully");
        }
    }

}
