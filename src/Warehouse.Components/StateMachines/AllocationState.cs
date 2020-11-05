namespace Warehouse.Components.StateMachines
{
    using System;
    using Automatonymous;


    public class AllocationState :
        SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        public Guid? HoldDurationToken { get; set; }
    }
}