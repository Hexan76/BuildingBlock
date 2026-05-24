namespace Framework.Localization;

public class LocalizeMemberAttribute : Attribute
{
    public LocalizeMemberAttribute(string localizeKey, int order = 100)
    {
        LocalizeKey = localizeKey;
        Order = order;
    }

    public string LocalizeKey { get; set; }
    public int Order { get; set; }

}
