using Elastic.Apm.NetCoreAll;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace dotnet_elastic_apm_example;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // add serilog to builder

        builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Application", "dotnet-elastic-apm-example")
                .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment.EnvironmentName)
                .Enrich.WithElasticApmCorrelationInfo()
                .WriteTo.Console(outputTemplate: "[{ElasticApmTraceId} {ElasticApmTransactionId} {ElasticApmSpanId} {Message:lj} {NewLine}{Exception}")
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(hostingContext.Configuration["ElasticConfiguration:Uri"] ?? "http://localhost:9200"))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "dotnet-elastic-apm-example-{0:yyyy.MM.dd}",
                    NumberOfShards = 2,
                    NumberOfReplicas = 1,
                    CustomFormatter = new EcsTextFormatter()
                });
        });

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseAllElasticApm();

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
    }
}
