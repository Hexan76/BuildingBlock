namespace Framework.RabbitMQ;

public sealed class PublishPipeline : IPublishPipeline
{
    private readonly IReadOnlyList<IPublishPipelineMiddleware> _middlewares;

    public PublishPipeline(IEnumerable<IPublishPipelineMiddleware> middlewares)
    {
        _middlewares = middlewares.ToList();
    }

    public Task ExecuteAsync(
        PublishContext context,
        Func<Task> terminalHandler,
        CancellationToken cancellationToken = default)
    {
        Func<Task> pipeline = terminalHandler;

        for (var index = _middlewares.Count - 1; index >= 0; index--)
        {
            var middleware = _middlewares[index];
            var next = pipeline;
            pipeline = () => middleware.InvokeAsync(context, next);
        }

        return pipeline();
    }
}

public sealed class ConsumePipeline : IConsumePipeline
{
    private readonly IReadOnlyList<IConsumePipelineMiddleware> _middlewares;

    public ConsumePipeline(IEnumerable<IConsumePipelineMiddleware> middlewares)
    {
        _middlewares = middlewares.ToList();
    }

    public Task ExecuteAsync(
        ConsumeContext context,
        Func<Task> terminalHandler,
        CancellationToken cancellationToken = default)
    {
        Func<Task> pipeline = terminalHandler;

        for (var index = _middlewares.Count - 1; index >= 0; index--)
        {
            var middleware = _middlewares[index];
            var next = pipeline;
            pipeline = () => middleware.InvokeAsync(context, next);
        }

        return pipeline();
    }
}
