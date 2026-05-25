namespace Framework.BuildingBlock.Contracts;

public enum FilterGroupOperatorDto
{
    Equal,
    NotEqual,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,

    // Collection operators
    In,
    ListContains,       // param.Contains(prop)
    AnyEqual,       // collection.Any(it => it == param)
    AnyNotEqual,
    AnyGreaterThan,
    AnyLessThan,
    ListAllEqual,
    ListAllNotEqual,
}
