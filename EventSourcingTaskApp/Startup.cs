using System;
using EventSourcingTaskApp.Infrastructure;
using EventStore.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventSourcingTaskApp;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {

        services.AddEventStoreClient( new Uri( "esdb://localhost:2113?tls=false" ) );

        //services
        //    .AddEventStoreClient(Configuration
        //        .GetSection("EventStore")
        //        .Get<string>());

        //var connectionString = Configuration.GetValue<string>("EventStore:ConnectionString");
        //var settings = EventStoreClientSettings
        //    .Create($"{connectionString}");
        //var eventStoreConnection = new EventStoreClient(settings);

        //var eventStoreConnection = EventStoreConnection.Create(
        //    connectionString: Configuration.GetValue<string>("EventStore:ConnectionString"),
        //    builder: EventStoreClientSettings.Create(),
        //    connectionName: Configuration.GetValue<string>("EventStore:ConnectionName"));

        //eventStoreConnection.ConnectAsync().GetAwaiter().GetResult();

        //services.AddSingleton(eventStoreConnection);
        services.AddTransient<AggregateRepository>();

        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "MVCCallWebAPI");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}