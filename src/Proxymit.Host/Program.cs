using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Proxymit.Core.Configs;
using Proxymit.Core.Extensions;
using Proxymit.Core.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;

namespace Proxymit.Host;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseKestrel(options =>
        {
            options.AddServerHeader = false;
        });

        builder.Services.Configure<ExposedPortConfig>(options => 
            builder.Configuration.GetSection("ExposedPorts").Bind(options));
        builder.Services.AddProxymit();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseMiddleware<ProxymitMiddleware>();

        app.MapGet("/", async context =>
        {
            await context.Response.WriteAsync("Hello World!");
        });

        app.Run();
    }
}
