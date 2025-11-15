
using FleetRent.Application.Bikes;
using FleetRent.Application.Drivers;
using FleetRent.Application.Rentals;
using FleetRent.Domain.Interfaces;
using FleetRent.Infrastructure.Data;
using FleetRent.Infrastructure.Messaging;
using FleetRent.Infrastructure.Messaging.Consumers;
using FleetRent.Infrastructure.Repositories;
using FleetRent.Infrastructure.Storage;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;
using System.Text.Json.Serialization;

namespace FleetRent.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting FleetRent API host");

                var builder = WebApplication.CreateBuilder(args);

                // Configure API behavior to suppress automatic model state validation
                builder.Services.Configure<ApiBehaviorOptions>(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

                builder.Host.UseSerilog((context, services, configuration) =>
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .Enrich.WithEnvironmentName()
                        .Enrich.WithMachineName()
                        .Enrich.WithProcessId()
                        .Enrich.WithThreadId());

                // Set default culture to pt-BR for entire application
                var supportedCultures = new[] { new CultureInfo("pt-BR") };

                builder.Services.Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture("pt-BR");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });

                builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    });

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                builder.Services.AddDbContext<FleetRentDbContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

                // Repo's
                builder.Services.AddScoped<IBikeRepository, BikeRepository>();
                builder.Services.AddScoped<IDriverRepository, DriverRepository>();
                builder.Services.AddScoped<IRentalRepository, RentalRepository>();
                builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

                builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FleetRentDbContext>());

                // Services
                builder.Services.AddScoped<IBikeService, BikeService>();
                builder.Services.AddScoped<IDriverService, DriverService>();
                builder.Services.AddScoped<IRentalService, RentalService>();

                // Storage
                builder.Services.AddScoped<IFileStorageService, FileStorageService>();

                // Messaging
                builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
                builder.Services.AddHostedService<BikeCreatedConsumer>();

                var app = builder.Build();

                // Migrations
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<FleetRentDbContext>();
                    db.Database.Migrate();
                }

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
            catch (Exception ex)
            {
                Log.Fatal(ex, "FleetRent API host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
