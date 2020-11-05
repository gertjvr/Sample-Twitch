namespace Warehouse.Components.StateMachines
{
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;


    public class AllocationStateMap : SagaClassMap<AllocationState>
    {
        protected override void Configure(EntityTypeBuilder<AllocationState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState).HasMaxLength(64);
        }
    }
}