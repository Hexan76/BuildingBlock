namespace Framework.BuildingBlock.Permissions;

public interface IPermissionProtectionService
{
    string Protect<T>(T data);
    T Unprotect<T>(string protectedData);
}
