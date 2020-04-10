using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Route ("api/Student")]
    [ApiController]
    
    public class StudentController : ControllerBase
    {


        [HttpGet]
        public IActionResult Index()
        {
            return Ok("boomer");
        }
    }
}