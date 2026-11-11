var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();
app.MapGet("/gateway-health", () => Results.Ok(new { application="Taller.Gateway", status="ok" }));
app.MapReverseProxy();
app.Run();
