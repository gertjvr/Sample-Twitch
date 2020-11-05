namespace Sample.Startup
{
    using System.Reflection;
    using Components.BatchConsumers;
    using Components.Consumers;
    using Components.CourierActivities;
    using Components.StateMachines;
    using Components.StateMachines.OrderStateMachineActivities;
    using MassTransit;
    using MassTransit.Azure.Storage;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using MassTransit.Platform.Abstractions;
    using Microsoft.Azure.Storage;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Warehouse.Contracts;


    public class SampleStartup :
        IPlatformStartup
    {
        public void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator, IServiceCollection services)
        {
            services.AddScoped<AcceptOrderActivity>();

            configurator.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
            configurator.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();
            configurator.AddConsumersFromNamespaceContaining<RoutingSlipBatchEventConsumer>();

            configurator.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
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

            configurator.AddRequestClient<AllocateInventory>();
        }

        public void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator,
            IBusRegistrationContext context)
            where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
            var account = CloudStorageAccount.Parse("");
            configurator.UseMessageData(account.CreateMessageDataRepository("attachments"));
        }
    }


    


    
}