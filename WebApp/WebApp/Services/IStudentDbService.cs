using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.DPOs.Requests;

namespace WebApp.Services
{
    public interface IStudentDbService
    {

        string EnrollStudent(EnrollStudentRequest req);
        string PromoteStudents(PromoteStudentRequest req);

    }
}
