using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Proxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

var builder = WebApplication.CreateBuilder(args);
// disable ssl cert check
ServicePointManager.ServerCertificateValidationCallback += 
    delegate(
        Object sender1,
        X509Certificate certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
    {
        return true;
    };
builder.Services.AddProxy(
    options =>
        {
            options.PrepareRequest = (originalRequest, message) =>
            {
                message.Headers.Add("X-Forwarded-Host", originalRequest.Host.Host);
                return Task.FromResult(0);
            };
        });

var app = builder.Build();

app.UseWebSockets().RunProxy(new Uri(args[0]));

app.Run();
