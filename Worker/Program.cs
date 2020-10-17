using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddTransient<IRepository, Repository>();
                    
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json")
                        .Build();
                    
                    var dbConnectionString = configuration.GetConnectionString("SqlServer");
                    var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ");
                    
                    services.AddScoped<IDbConnection>(sp => new SqlConnection(dbConnectionString));
                    services.AddScoped(sp => new ConnectionFactory {HostName = rabbitMqConnectionString}.CreateConnection());
                });
    }
}