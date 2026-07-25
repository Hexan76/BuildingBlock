namespace Framework.BuildingBlock.Abstracts;

public interface IClock
{
    DateTime Now { get; }

    DateTime Normalize(DateTime dateTime);

    DateTime? Normalize(DateTime? dateTime);
}
