namespace Framework.BuildingBlock.Application.Contracts;

public sealed class ResultApi<TResponseMessage> : MessageContract<TResponseMessage>
where TResponseMessage : class
{
    public bool Success { get; set; } = true;
    public TResponseMessage Result { get; set; }
    public Pagination Pagination { get; set; }
    public ErrorDetails? Error { get; set; }
    public Snackbar? Snackbar { get; set; }
}

public class Pagination
{
    public int Total { get; set; }
    public int CurrentPage { get; set; }
}

public class ErrorDetails
{
    public int Code { get; set; }
    public int HttpCode { get; set; }
    public string Message { get; set; }
}

public class Snackbar
{
    public string Type { get; set; }
    public string Message { get; set; }
}
