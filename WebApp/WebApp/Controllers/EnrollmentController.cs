﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
           
            string result = _service.EnrollStudent(request);  
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetEnrollments()
        {
            return Ok("Welcome to enrolling.");

        }
    }
}