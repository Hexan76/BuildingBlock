using Framework.BuildingBlock.Application.Contracts;
using MediatR;

namespace Framework.BuildingBlock.HttpApi;

public abstract class BaseFileEndpoint<TRequest> : Endpoint<TRequest>
    where TRequest : class, IFrameworkRequest<FileResponse>
{
    public IMediator Mediator { get; set; } = null!;

    public override async Task HandleAsync(TRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(req, ct);

        var stream = new MemoryStream(result.Data.Content);
        await Send.StreamAsync(stream, result.Data.FileName, result.Data.Content.Length, result.Data.ContentType);
    }
}