namespace Sample.Components.Consumers
{
    using System.Linq;
    using System.Threading.Tasks;
    using MassTransit;
    using MassTransit.Courier.Contracts;
    using Microsoft.Extensions.Logging;


    public class RoutingSlipEventConsumer :
        IConsumer<RoutingSlipFaulted>,
        IConsumer<RoutingSlipActivityCompleted>
    {
        readonly ILogger<RoutingSlipEventConsumer> _logger;

        public RoutingSlipEventConsumer(ILogger<RoutingSlipEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<RoutingSlipActivityCompleted> context)
        {
            _logger.LogInformation("Routing Slip Activity Completed: {TrackingNumber} {ActivityName}", context.Message.TrackingNumber,
                context.Message.ActivityName);

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoutingSlipFaulted> context)
        {
            _logger.LogInformation("Routing Slip Faulted: {TrackingNumber} {ExceptionInfo}", context.Message.TrackingNumber,
                context.Message.ActivityExceptions.FirstOrDefault());

            return Task.CompletedTask;
        }
    }
}