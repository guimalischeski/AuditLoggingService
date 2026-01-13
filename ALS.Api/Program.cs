using ALS.Core.Constants;
using ALS.Core.Interfaces;
using ALS.Core.Observability;
using ALS.Infrastructure.Health;
using ALS.Infrastructure.Persistence;
using ALS.Infrastructure.Services;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var cs = builder.Configuration.GetConnectionString(Constants.ConfigurationKeys.AuditDb);

builder.Services.AddDbContext<AuditDbContext>(opt =>
{
    opt.UseSqlServer(cs);
});

var awsSection = builder.Configuration.GetSection(Constants.ConfigurationKeys.Aws);
var serviceUrl = awsSection[Constants.ConfigurationKeys.ServiceUrl];
var region = awsSection[Constants.ConfigurationKeys.Region];
builder.Services.AddSingleton<IAmazonSQS>(_ =>
{
    var cfg = new AmazonSQSConfig
    {
        ServiceURL = serviceUrl,
        AuthenticationRegion = region
    };

    return new AmazonSQSClient(cfg);
});

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString(Constants.ConfigurationKeys.AuditDb)!, name: "db")
    .AddCheck<SqsHealthCheck>("sqs");

builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditQueryService, AuditQueryService>();
builder.Services.AddScoped<IAuditIngestionService, AuditIngestionService>();

builder.Services.AddSingleton(AuditMetrics.Instance);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseHttpMetrics();
app.MapMetrics("/metrics");
app.UseMetricServer(url: "/metrics/als", registry: AuditMetrics.Registry);

app.MapControllers();

app.Run();
