using CanadaImmigration.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IExpressEntryDrawService, ExpressEntryDrawService>(client =>
{
    client.BaseAddress = new Uri("https://can-ee-draws.karanjit-sagun01.workers.dev/api/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IProofOfFundsService, ProofOfFundsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
