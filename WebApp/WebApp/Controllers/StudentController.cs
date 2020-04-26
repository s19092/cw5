using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApp.DPOs.Requests;
using WebApp.Services;

namespace WebApp.Controllers
{
    [Route ("api/Student")]
    [ApiController]
    


    public class StudentController : ControllerBase
    {

        public IConfiguration Configuration { get; set; }

        private IStudentDbService _service;

        public StudentController(IConfiguration conf,IStudentDbService service)
        {
            _service = service;
            Configuration = conf;
        }

        [HttpGet]
        [Authorize(Roles ="admin")]
        public IActionResult Index()
        {
            return Ok("boomer");
        }


        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {

            string result = _service.Login(request);
            if (result.Contains("Failed:"))
                return Unauthorized("require login and password");



            var claims = new[] {

            new Claim(ClaimTypes.NameIdentifier,"1"),
            new Claim(ClaimTypes.Name, "jan123"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "student")
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var jwt = new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            };

           string s = _service.UpdateToken(jwt.refreshToken,request.Login);


            return Ok(jwt);

        }

        [HttpPost("refresh-token/{refridger}")]
        public IActionResult RefreshToken(string refridger)
        {

            string result =_service.RefreshToken(refridger);
            if (result.Contains("Failed:"))
                return Unauthorized("Not valid token");


            var claims = new[] {

            new Claim(ClaimTypes.NameIdentifier,"1"),
            new Claim(ClaimTypes.Name, "jan123"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "student"),
            new Claim(ClaimTypes.Role, "employee")
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var jwt = new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            };

            

            return Ok(jwt);



        }
    }
}