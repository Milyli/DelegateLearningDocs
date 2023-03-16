using DelegateLearningDocs.Data;
using DelegateLearningDocs.Hangfire.QueueManagers;
using DelegateLearningDocs.Hangfire.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("UnsecureRelativityClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
        });

builder.Services.AddControllers();

builder.Services.AddSingleton<ISlackMessageQueueManager, SlackMessageQueueManager>();
builder.Services.AddSingleton<ISlackMessageTask, SlackMessageTask>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure EF Context
builder.Services.AddDbContext<DelegateLearningContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Add Hangfire
builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("Default"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));
builder.Services.AddHangfireServer();

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

// Initialize and Clean-up Databases

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    DelegateLearningContextInitializer.Initialize(services);
}

app.UseHangfireDashboard("/hangfire");

app.Run();
