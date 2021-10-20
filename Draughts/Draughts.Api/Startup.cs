using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Draughts.Api
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                // In development we can tell the browser to ignore CORS because our site isn't actually on
                // the internet and it's more convenient to not configure the origin on the frontend each time
                options.AddPolicy("Development",
                    builder =>
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .Build());

                // In production we tell the browser to only allow requests from our frontend
                // which will be on the same origin (this web api behind /api/)
                options.AddPolicy("Production", builder => builder.Build());
            });
            
            // We use an anti-forgery system to protect against CSRF attacks.
            // The token is received via a GET request (which only our frontend will be able to make)
            // and is sent with every subsequent request to be validated by the server.
            // Doing this we can guarantee that every request is coming from our website and not
            // a malicious website making requests to the API on behalf of our users.
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
            
            services.AddControllers(options => 
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
        }
        
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseCors("Development");
            else
            {
                app.UseCors("Production");
            }
            
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
