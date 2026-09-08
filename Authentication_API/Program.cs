using Authentication_API.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Authentication_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            //2 guards set up that check at every request before it reaches controllers
            app.UseAuthentication();/* Guard 1 - Who is this? Looks at the incoming request's Cookie header
                                    if there is, decrypts it and uses it to set who the current request's user is (teacher/student) */

            app.UseAuthorization(); /* Guard 2 - Are they allowed to do this? 
                                     * Looks at whatever [Authorize] says compares the required role
                                     against what Guard 1 found, lets the request through, or blocks it */


            app.MapControllers();
            app.Run();
        }
    }
}
