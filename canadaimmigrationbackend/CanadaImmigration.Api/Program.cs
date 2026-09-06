using CanadaImmigration.Api.Services;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(AngularDevCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();