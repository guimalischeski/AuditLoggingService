using ALS.Consumer;
using ALS.Core.Constants;
using ALS.Core.Interfaces;
using ALS.Infrastructure.Persistence;
using ALS.Infrastructure.Services;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

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

builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditIngestionService, AuditIngestionService>();
builder.Services.AddHostedService<SqsAuditConsumer>();

builder.Build().Run();