using System;
using Draughts.Api.Games;
using Draughts.Api.Hubs;
using Draughts.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Draughts.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
        
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }
        
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            // In development we need to specify that the React frontend (which runs on :3000) can make
            // requests to this web API (which runs on :5001) because all requests will be cross-origin.
            options.AddPolicy("Development",
                builder =>
                    builder.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .Build());

            // In production we can specify origins via the config file.
            options.AddPolicy("Production", 
                builder => 
                    builder.WithOrigins(_configuration.GetSection("Cors").Get<string[]>())
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .Build());
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
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        services.AddMvc(options =>
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
            
        services.AddControllers();

        services.AddSignalR();

        services.AddSingleton<Random>();
        services.AddSingleton<GameService>();
        services.AddTransient<LocalMultiplayerGame>();
        services.AddTransient<OnlineMultiplayerGame>();
        services.AddTransient<ComputerGame>();
        services.AddTransient<MiniMaxEngine>();
        services.AddTransient<RandomEngine>();
        services.AddAutoMapper(typeof(Startup).Assembly);
    }
        
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) app.UseCors("Development");
        else app.UseCors("Production");

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<DraughtsHub>("hub");
        });
    }
}
