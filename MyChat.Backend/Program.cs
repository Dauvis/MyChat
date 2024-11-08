using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MyChat.Security;
using System.Text.Json.Serialization;
using System.Text.Json;
using MyChat.Backend;
using Serilog;
using MyChat.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog(dispose: true);
});

builder.Services.AddBackendServices();
builder.Services.AddCommonServices();
builder.Services.AddLogicServices();
builder.Services.AddSecurityServices();
builder.Services.AddDataServices();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET_KEY"]!)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:ValidAudience"],
        ValidateLifetime = true
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

try
{
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<JwtMiddleware>();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    // Log any startup errors
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    // Ensure the log is flushed and closed properly on shutdown
    Log.CloseAndFlush();
}
