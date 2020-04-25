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

namespace WebApp.Controllers
{
    [Route ("api/Student")]
    [ApiController]
    
    public class StudentController : ControllerBase
    {

        public IConfiguration Configuration { get; set; }

        public StudentController(IConfiguration conf)
        {
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

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            });

        }
    }
}