var builder = WebApplication.CreateBuilder(args);

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
