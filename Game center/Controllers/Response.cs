using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponseController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            var data = new SampleData
            {
                Id = 1,
                Name = "Sample Item",
                Description = "This is a sample item."
            };

            var response = new Response
            {
                Message = "Data retrieved successfully.",
                Status = "Success",
                Data = data
            };

            return Ok(response);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SampleData data)
        {
            var savedData = new SampleData
            {
                Id = 1,
                Name = data.Name,
                Description = data.Description
            };

            var response = new Response
            {
                Message = "Data saved successfully.",
                Status = "Success",
                Data = savedData
            };

            return Ok(response);
        }

        public class SampleData
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
        public class Response
        {
            public string Message { get; set; }
            public string Status { get; set; }
            public object Data { get; set; }
        }
    }
}