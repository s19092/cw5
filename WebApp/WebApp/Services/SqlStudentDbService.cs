﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using WebApp.DPOs.Requests;
using WebApp.Models;

namespace WebApp.Services
{
    public class SqlStudentDbService : IStudentDbService
    {

        private string ConnectionString = "Data Source=db-mssql;Initial Catalog=s19092;Integrated Security=True";

        public string EnrollStudent(EnrollStudentRequest req)
        {            
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                
                StringBuilder result = new StringBuilder();
                connection.Open();

                SqlTransaction trans = connection.BeginTransaction("Samp");

                using (SqlCommand command = new SqlCommand("SELECT * FROM Studies WHERE name = @name", connection))
                {
                    command.Parameters.AddWithValue("name", req.Studies);
                    command.Transaction = trans;

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            reader.Close();
                            trans.Rollback();
                            return "Failed: There is not such " + req.Studies + " in table.";
                        }

                    }
                }
                using (SqlCommand command = new SqlCommand("SELECT * FROM Enrollment e INNER JOIN Studies s ON s.idStudy = e.idStudy WHERE s.name = @name;" +
                    " SELECT * FROM Studies WHERE name = @name;" +
                    " SELECT MAX(idEnrollment) + 1 FROM Enrollment;", connection))
                {

                    int IdEnrollment = -1;
                    int IdStudy = -1;
                    command.Parameters.AddWithValue("name", req.Studies);
                    command.Transaction = trans;
                    using (var reader = command.ExecuteReader())
                    {

                        if (!reader.Read())
                        {
                            result.Append("Needed to create table.\n");
                            reader.NextResult();
                            reader.Read();
                            IdStudy = (int)reader[0];
                            reader.NextResult();
                            reader.Read();
                            IdEnrollment = (int)reader[0];
                            String date = DateTime.Now.ToString(); 
                            command.CommandText = "INSERT INTO Enrollment (IdEnrollment,Semester,IdStudy,StartDate) VALUES (@idenrollment,1,@idstudy,'" + date.Substring(0, date.IndexOf(" ")).ToString() + "')";
                            command.Parameters.AddWithValue("idenrollment", IdEnrollment);
                            command.Parameters.AddWithValue("idstudy", IdStudy);
                            reader.Close();
                            command.ExecuteNonQuery();

                        }
                        DateTime d = Convert.ToDateTime(req.BirthDate);
                        String sDate;
                        sDate = d.Date.ToString("MM-dd-yyyy").Substring(0,d.Date.ToString().IndexOf(" "));
                        reader.Close();
                        command.CommandText = "INSERT INTO Student (IndexNumber,FirstName,LastName,BirthDate,IdEnrollment) " +
                            "VALUES (@index,@first_name,@last_name,'" +sDate + "',@id_enrollment)";
                        command.Parameters.AddWithValue("index", req.IndexNumber);
                        command.Parameters.AddWithValue("first_name", req.FirstName);
                        command.Parameters.AddWithValue("last_name", req.LastName);
                        command.Parameters.AddWithValue("id_enrollment", IdEnrollment);
                        try
                        {
                            command.ExecuteNonQuery();
                        }catch(SqlException e)
                        {
                            trans.Rollback();
                            return "Failed: Not unique index.";
                        }
                        trans.Commit();
                        result.Append("ADDED");
                        return result.ToString();
                    }
                }
                    
            }

        }

        public Student GetStudent(string indexNumber)
        {

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {

                connection.Open();
                using (var command = new SqlCommand("SELECT * FROM Student WHERE IndexNumber = @index",connection))
                {

                    command.Parameters.AddWithValue("index", indexNumber);

                    using (var reader = command.ExecuteReader())
                    {

                        if (!reader.Read())
                            return null;
                        return new Student
                        {

                            IndexNumber = indexNumber,
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            BirthDate = (DateTime)reader["BirthDate"],
                            IdEnrollment = (int)reader["IdEnrollment"]
                        };

                       

                    }

                }
              


            }

            return null;
        }


        public string Login(LoginRequest request)
        {

            var response = new StringBuilder();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string password = null, salt = null;
                using(var command = new SqlCommand("SELECT * FROM Student WHERE indexNumber = @login AND password IS NULL",connection))
                {
                    command.Parameters.AddWithValue("login", request.Login);
                    using (var reader = command.ExecuteReader())
                    {

                        if (reader.Read())
                        {
                            reader.Close();
                            byte[] saltB = new byte[128 / 8];
                            using (var generator = RandomNumberGenerator.Create())
                            {
                                generator.GetBytes(saltB);
                            }
                            salt = Convert.ToBase64String(saltB);
                          
                            password = createPassword(request.Password, salt);
                           
                            
                        }
                                                                        
                    }


                }
                if(password != null)
                {

                    using (var command = new SqlCommand
                        ("UPDATE student SET password = @password , salt = @salt WHERE indexnumber = @index",connection))
                    {

                        command.Parameters.AddWithValue("salt",salt);
                        command.Parameters.AddWithValue("password", password);
                        command.Parameters.AddWithValue("index", request.Login);
                        command.ExecuteNonQuery();
                       

                    }

                }
                

              
                using (var command = new SqlCommand("SELECT * FROM Student WHERE indexNumber = @login",connection))
                {

                    command.Parameters.AddWithValue("login", request.Login);
                  
                    using (var reader = command.ExecuteReader())
                    {

                        if (!reader.Read())
                            return "Failed:";
                        response.Append("Connected:");
                        salt = reader["salt"].ToString();
                        password = reader["password"].ToString();
                    }

                }

                return (request.Password + " " + createPassword(request.Password, salt));

                if (!createPassword(request.Password, salt).Equals(password)) 
                    return "Failed:";
                return "Connect:";
               
            }
           


            return null;

        }

        public string createPassword(string pass,string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(

                               password: pass,
                               salt: Encoding.UTF8.GetBytes(salt),
                               prf: KeyDerivationPrf.HMACSHA512,
                               iterationCount: 100,
                               numBytesRequested: 256 / 8);
            return Convert.ToBase64String(valueBytes);
        }


        public string PromoteStudents(PromoteStudentRequest request)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("EXECUTE jd0 @studies_name , @semester",connection))
                {

                    command.Parameters.AddWithValue("studies_name", request.Studies);
                    command.Parameters.AddWithValue("semester", request.Semester);
                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {

                            if (!reader.Read())
                            {
                                return "Failed: Unable to read.";
                            }
                            Enrollment enrollment = new Enrollment
                            {
                                IdEnrollment = (int)reader["IdEnrollment"],
                                Semester = (int)reader["Semester"],
                                IdStudy = (int)reader["IdStudy"],
                                StartDate = (DateTime)reader["StartDate"]
                            };

                            return enrollment.ToString();

                        }
                    }catch(SqlException e)
                    {
                       
                        return "Failed: Check if procedure exists. " + e.Message;

                    }
               }
            }
        }

        public string RefreshToken(string refresh)
        {
            var respond = new StringBuilder();

            using(var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using(var command = new SqlCommand("SELECT * FROM Student WHERE refresh = @reffo",connection))
                {

                    command.Parameters.AddWithValue("reffo", refresh);
                    using (var reader = command.ExecuteReader())
                    {

                        if (!reader.Read())
                            return "Failed:";
                        
                    }


                }

            }

            return "Connected:";
            
        }

        public string UpdateToken( Guid refreshToken,string index)
        {

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE Student SET refresh = @ref WHERE indexNumber = @index",connection))
                {

                    command.Parameters.AddWithValue("ref", refreshToken.ToString());
                    command.Parameters.AddWithValue("index", index);
                    command.ExecuteNonQuery();
                    return "JD";


                }

            }
        
            
        }


    }
}
