namespace Framework.BuildingBlock.Application.Contracts;

public class FrameworkValidation
{
    public FrameworkValidation()
    {

    }

    public FrameworkValidation(string propertyName, string errorMessage) : this(propertyName, errorMessage, null)
    {

    }

    public FrameworkValidation(string propertyName, string errorMessage, object attemptedValue)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        AttemptedValue = attemptedValue;
    }

    public string PropertyName { get; set; }

    public string ErrorMessage { get; set; }

    public object AttemptedValue { get; set; }

    public object CustomState { get; set; }

    public string ErrorCode { get; set; }

    public Dictionary<string, object> FormattedMessagePlaceholderValues { get; set; }

    public override string ToString()
    {
        return ErrorMessage;
    }

}
