using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using WebApp.Middlewares;
using WebApp.Models;
using WebApp.Services;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            services.AddSwaggerGen(config =>
            {

                config.SwaggerDoc("v1", new OpenApiInfo { Title = " Students App API", Version = "v1"});

            });


            services.AddTransient<IStudentDbService, SqlStudentDbService>();

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IStudentDbService service)
        {
    

            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "Students App API");

            });



            app.UseMiddleware<LoggingMiddleware>();
            app.Use(async (context, next) =>
            {

                if (!context.Request.Headers.ContainsKey("IndexNumber"))
                {

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Musisz podac numer indexu istniejacego studenta");
                    return;

                }



                var index = context.Request.Headers["IndexNumber"];

                Student student = service.GetStudent(index);

                if (student == null)
                {
                    await context.Response.WriteAsync("Nie ma takiego studenta.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                await context.Response.WriteAsync(student.ToString());

                await next();
            });
            
            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
