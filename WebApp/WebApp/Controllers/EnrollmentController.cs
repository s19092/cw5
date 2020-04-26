using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.DPOs.Requests;
using WebApp.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApp.Controllers
{
    [Route("api/Enrollment")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private IStudentDbService _service;

        public EnrollmentController(IStudentDbService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
           
            string result = _service.EnrollStudent(request);
            if (result.Contains("Failed:"))
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{promotions}")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(PromoteStudentRequest request)
        {
            string result = _service.PromoteStudents(request);
            if(result.Contains("Failed:"))
                return BadRequest(result);
            return Ok(result);

        }
        [HttpGet]
        public IActionResult GetEnrollments()
        {   
            return Ok("Welcome to enrolling.");

        }
    }
}
