namespace Warehouse.Startup
{
    using System.Reflection;
    using Components.Consumers;
    using Components.StateMachines;
    using MassTransit;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using MassTransit.Platform.Abstractions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;


    public class WarehouseStartup :
        IPlatformStartup
    {
        public void ConfigureMassTransit(IServiceCollectionBusConfigurator configurator, IServiceCollection services)
        {
            configurator.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();
            configurator.AddSagaStateMachine<AllocationStateMachine, AllocationState>(typeof(AllocateStateMachineDefinition))
                .EntityFrameworkRepository(r =>
                {
                    r.AddDbContext<DbContext, AllocationStateDbContext>((provider,builder) =>
                    {
                        builder.UseSqlServer("Server=tcp:gertjvr.database.windows.net,1433;Initial Catalog=gertjvr;Persist Security Info=False;User ID=gertjvr;Password=Works4me!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;", m =>
                        {
                            m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            m.MigrationsHistoryTable($"__{nameof(AllocationStateDbContext)}");
                        });
                    });
                });
        }

        public void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator, IBusRegistrationContext context)
            where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
        }
    }
}