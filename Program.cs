using MdsPoc.Application.Interfaces;
using MdsPoc.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDecisionEvaluationService, DecisionEvaluationService>();
builder.Services.AddScoped<ITemperatureService, TemperatureService>();
builder.Services.AddScoped<IReEvaluationService, ReEvaluationService>();
builder.Services.AddScoped<CatalogService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();