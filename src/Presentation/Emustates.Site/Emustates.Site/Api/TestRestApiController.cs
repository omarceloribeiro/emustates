using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Emustates.Site.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestRestApiController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        // endpoint of healhth check
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy", timestamp = System.DateTime.UtcNow });
        }

        // endpoint to simulate an error
        [HttpGet("error")]
        public IActionResult Error()
        {
            throw new System.Exception("This is a simulated error for testing purposes.");
        }

        // endpoint to test problem details response
        [HttpGet("problem")]
        public IActionResult Problem()
        {
            return Problem(detail: "This is a test problem details response.", statusCode: StatusCodes.Status400BadRequest, title: "Test Problem");
        }

        // endpoint to echo back a message
        [HttpGet("echo")]
        public IActionResult Echo([FromQuery] string message)
        {
            return Ok(new { echoedMessage = message });
        }

        // endpoint to test json response 
        [HttpGet("json")]
        public IActionResult JsonResponse()
        {
            var data = new
            {
                Name = "Emustates Test API",
                Version = "1.0.0",
                Features = new[] { "Ping", "Health Check", "Error Simulation", "Problem Details", "Echo" }
            };
            return new JsonResult(data);
        }

        // endpoint to test not found response
        [HttpGet("notfound")]
        public IActionResult NotFoundResponse()
        {
            return NotFound(new { message = "The requested resource was not found." });
        }

        // endpoint to test unauthorized response
        [HttpGet("unauthorized")]
        public IActionResult UnauthorizedResponse()
        {
            return Unauthorized(new { message = "You are not authorized to access this resource." });
        }

        // endpoint to test forbidden response
        [HttpGet("forbidden")]
        public IActionResult ForbiddenResponse()
        {
            return Forbid();
        }

        // endpoint to test bad request response
        [HttpGet("badrequest")]
        public IActionResult BadRequestResponse()
        {
            return BadRequest(new { message = "This is a bad request example." });
        }

        // endpoint to test model validation of a simple product model, where title is required
        [HttpPost("validate")]
        public IActionResult ValidateModel([FromBody] ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(new { message = "Model is valid." });
        }
        public class ProductModel
        {
            [System.ComponentModel.DataAnnotations.Required]
            public string Title { get; set; } = string.Empty;
            public decimal Price { get; set; }
        }

        // endpoint to test delay
        [HttpGet("delay")]
        public IActionResult Details()
        {
            System.Threading.Thread.Sleep(2000); // 2 seconds delay
            return Ok(new { message = "Response after delay." });
        }

        // endpoint to test endpoint name and endpoint path
        [HttpGet("info")]
        public IActionResult Info()
        {
            var endpoint = HttpContext.GetEndpoint();
            var endpointName = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.EndpointNameMetadata>()?.EndpointName;
            var endpointPath = endpoint?.DisplayName;
            return Ok(new { endpointName, endpointPath });

        }

        // endpoint to test [Route] attribute along with [HttpGet]
        [HttpGet("route-test", Name = "route-test-name")]
        public IActionResult RouteTest()
        {
            return Ok(new { message = "Route attribute test successful." });
        }

    }
}
