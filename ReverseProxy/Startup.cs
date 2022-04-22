public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddProxy(options =>
        {
            options.PrepareRequest = (originalRequest, message) =>
            {
                message.Headers.Add("X-Forwarded-Host", originalRequest.Host.Host);
                return Task.FromResult(0);
            };
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSockets().RunProxy(new Uri("https://example.com"));
    }
}
