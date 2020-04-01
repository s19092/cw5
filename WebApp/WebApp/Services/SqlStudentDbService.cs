using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.DPOs.Requests;

namespace WebApp.Services
{
    public class SqlStudentDbService : IStudentDbService
    {
        public string EnrollStudent(EnrollStudentRequest req)
        {
            return "Failed";
        }

        public string PromoteStudents(int semester, string studies)
        {
            return "Failed"; 
        }
    }
}
