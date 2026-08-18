using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Framework.BuildingBlock.HttpApi;

public class AddHeaderOperationProcessor : IOperationProcessor
{
    private readonly List<OpenApiHeaderOption> _headers;

    public AddHeaderOperationProcessor(List<OpenApiHeaderOption> headers)
    {
        _headers = headers;
    }

    public bool Process(OperationProcessorContext context)
    {
        if (_headers == null || !_headers.Any())
            return true;

        foreach (var header in _headers)
        {
            var parameter = new OpenApiParameter
            {
                Name = header.Name,
                Kind = OpenApiParameterKind.Header,
                IsRequired = header.Required,
                Description = header.Description,
                Schema = new JsonSchema { Type = JsonObjectType.String }
            };

            context.OperationDescription.Operation.Parameters.Add(parameter);
        }

        return true;
    }
}
