using System.Security.Authentication;
using SDV.Infra.IoC;

var builder = WebApplication.CreateBuilder(args);

Bootstrap.StartIoC(builder.Services, builder.Configuration);

builder.Services.AddControllers();

builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false;
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.SslProtocols = SslProtocols.Tls13;
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapControllers();

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}
