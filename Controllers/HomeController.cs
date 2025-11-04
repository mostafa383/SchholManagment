using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace SmartSchool.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        // Simple in-memory model and storage for demonstration.


        // GET: api/home
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok("Welcome to SmartSchool API!");
        }

    }
}