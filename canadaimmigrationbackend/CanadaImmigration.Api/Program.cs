using System.Threading.RateLimiting;
using CanadaImmigration.Api.Data;
using CanadaImmigration.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevCorsPolicy = "AngularDevCorsPolicy";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var externalApiBaseUrl = builder.Configuration["ExternalApi:BaseUrl"]
    ?? throw new InvalidOperationException("ExternalApi:BaseUrl não configurado em appsettings.json.");

builder.Services.AddHttpClient<IExpressEntryDrawService, ExpressEntryDrawService>(client =>
{
    client.BaseAddress = new Uri(externalApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IProofOfFundsService, ProofOfFundsService>();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=canadaimmigration.db";

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddScoped<ISubscriberService, SubscriberService>();

var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"]
    ?? throw new InvalidOperationException("Cors:AllowedOrigin não configurado em appsettings.json.");

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigin)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Credenciais de SMTP nunca ficam no código: vêm de appsettings.json (Development)
// ou de variáveis de ambiente / user-secrets em produção. Ver seção "Email" no appsettings.
builder.Services.Configure<EmailNotifierConfig>(builder.Configuration.GetSection("Email"));

// Rate limit por IP no endpoint de inscrição (proteção contra spam de cadastros).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    var permitLimit = builder.Configuration.GetValue<int?>("RateLimiting:SubscribePermitLimit") ?? 5;
    var windowMinutes = builder.Configuration.GetValue<int?>("RateLimiting:SubscribeWindowMinutes") ?? 1;

    options.AddPolicy("subscribe", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(windowMinutes),
                QueueLimit = 0
            }));
});
builder.Services.AddSingleton<EmailNotifier>();
builder.Services.AddHostedService<ExpressEntryPollingService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(AngularDevCorsPolicy);

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();