namespace Sample.Service
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Components.BatchConsumers;
    using Components.Consumers;
    using Components.CourierActivities;
    using Components.StateMachines;
    using Components.StateMachines.OrderStateMachineActivities;
    using MassTransit;
    using MassTransit.Azure.ServiceBus.Core;
    using MassTransit.Azure.Storage;
    using MassTransit.Courier.Contracts;
    using MassTransit.Definition;
    using MassTransit.EntityFrameworkCoreIntegration;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Azure.Storage;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Events;
    using Warehouse.Contracts;


    class Program
    {
        static DependencyTrackingTelemetryModule _module;
        static TelemetryClient _telemetryClient;

        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                        config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    _module = new DependencyTrackingTelemetryModule();
                    _module.IncludeDiagnosticSourceActivities.Add("MassTransit");

                    TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
                    configuration.InstrumentationKey = "0bd7dab0-5809-4ce5-b477-c89271124a54";
                    configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

                    _telemetryClient = new TelemetryClient(configuration);

                    _module.Initialize(configuration);

                    services.AddScoped<AcceptOrderActivity>();

                    services.AddScoped<RoutingSlipBatchEventConsumer>();

                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                        cfg.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();

                        cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                            .EntityFrameworkRepository(r =>
                            {
                                r.AddDbContext<DbContext, OrderStateDbContext>((provider,builder) =>
                                {
                                    builder.UseSqlServer("Server=tcp:gertjvr.database.windows.net,1433;Initial Catalog=gertjvr;Persist Security Info=False;User ID=gertjvr;Password=Works4me!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;", m =>
                                    {
                                        m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                                        m.MigrationsHistoryTable($"__{nameof(OrderStateDbContext)}");
                                    });
                                });
                            });

                        cfg.UsingAzureServiceBus(ConfigureBus);

                        cfg.AddRequestClient<AllocateInventory>();
                    });

                    services.AddHostedService<MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddSerilog(dispose: true);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                });

            if (isService)
                await builder.UseWindowsService().Build().RunAsync();
            else
                await builder.RunConsoleAsync();

            _telemetryClient?.Flush();
            _module?.Dispose();

            Log.CloseAndFlush();
        }

        static void ConfigureBus(IBusRegistrationContext context, IServiceBusBusFactoryConfigurator configurator)
        {
            var account = CloudStorageAccount.Parse("");
            configurator.Host("");
            configurator.UseMessageData(account.CreateMessageDataRepository("attachments"));
            configurator.UseServiceBusMessageScheduler();

            configurator.ReceiveEndpoint(KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(), e =>
            {
                e.PrefetchCount = 20;

                e.Batch<RoutingSlipCompleted>(b =>
                {
                    b.MessageLimit = 10;
                    b.TimeLimit = TimeSpan.FromSeconds(5);

                    b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(context);
                });
            });

            configurator.ConfigureEndpoints(context);
        }
    }
}