using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;

namespace WinTool.Updater;

internal class IOHelper
{
    public static void PrepareUpdateDirectory(string dataDirectory, string updateDirectory)
    {
        var dataDirectoryInfo = Directory.CreateDirectory(dataDirectory);

        if (dataDirectoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new InvalidDataException("The WinTool data directory cannot be a reparse point.");

        var administrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        var authenticatedUsers = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
        var system = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        var dataSecurity = new DirectorySecurity();

        dataSecurity.SetAccessRuleProtection(true, false);
        dataSecurity.SetOwner(administrators);
        dataSecurity.AddAccessRule(CreateFullControlRule(administrators));
        dataSecurity.AddAccessRule(CreateFullControlRule(system));
        dataSecurity.AddAccessRule(new FileSystemAccessRule(authenticatedUsers, FileSystemRights.ReadAndExecute | FileSystemRights.Write, AccessControlType.Allow));
        dataSecurity.AddAccessRule(new FileSystemAccessRule(authenticatedUsers, FileSystemRights.Modify, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
        dataDirectoryInfo.SetAccessControl(dataSecurity);

        if (File.Exists(updateDirectory))
            File.Delete(updateDirectory);

        var existingUpdateDirectory = new DirectoryInfo(updateDirectory);

        if (existingUpdateDirectory.Exists && existingUpdateDirectory.Attributes.HasFlag(FileAttributes.ReparsePoint))
            existingUpdateDirectory.Delete();

        var directoryInfo = Directory.CreateDirectory(updateDirectory);
        var security = new DirectorySecurity();

        security.SetAccessRuleProtection(true, false);
        security.SetOwner(administrators);
        security.AddAccessRule(CreateFullControlRule(administrators));
        security.AddAccessRule(CreateFullControlRule(system));
        directoryInfo.SetAccessControl(security);

        ClearDirectory(directoryInfo);
    }

    private static FileSystemAccessRule CreateFullControlRule(IdentityReference identity)
    {
        return new FileSystemAccessRule(identity, FileSystemRights.FullControl,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
    }

    public static bool VerifyFile(string path, long expectedSize, byte[] expectedHash)
    {
        var fileInfo = new FileInfo(path);

        if (!fileInfo.Exists || fileInfo.Length != expectedSize)
            return false;

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var actualHash = SHA256.HashData(stream);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static void ClearDirectory(DirectoryInfo directory)
    {
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            if (entry is DirectoryInfo childDirectory && !entry.Attributes.HasFlag(FileAttributes.ReparsePoint))
                ClearDirectory(childDirectory);

            entry.Delete();
        }
    }
}
