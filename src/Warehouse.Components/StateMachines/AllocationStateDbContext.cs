namespace Warehouse.Components.StateMachines
{
    using System.Collections.Generic;
    using MassTransit.EntityFrameworkCoreIntegration;
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;


    public class AllocationStateDbContext : 
        SagaDbContext
    {
        public AllocationStateDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new AllocationStateMap(); }
        }
    }
}