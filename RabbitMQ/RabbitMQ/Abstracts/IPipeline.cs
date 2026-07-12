namespace Framework.RabbitMQ;

public delegate Task PublishPipelineDelegate(PublishContext context, Func<Task> next);

public delegate Task ConsumePipelineDelegate(ConsumeContext context, Func<Task> next);

public interface IPublishPipeline
{
    Task ExecuteAsync(PublishContext context, Func<Task> terminalHandler, CancellationToken cancellationToken = default);
}

public interface IConsumePipeline
{
    Task ExecuteAsync(ConsumeContext context, Func<Task> terminalHandler, CancellationToken cancellationToken = default);
}

public interface IPublishPipelineMiddleware
{
    Task InvokeAsync(PublishContext context, Func<Task> next);
}

public interface IConsumePipelineMiddleware
{
    Task InvokeAsync(ConsumeContext context, Func<Task> next);
}
