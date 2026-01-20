namespace SftpGo.Service;
public interface IFileTransfer  
{
    void ActivateUsers(List<string> usernames);
    Task AddUser(string ftpAccountName, string password, long projectId);
    Task CreateFolder(long folderId);
    void DeactivateUsers(List<string> usernames);
    void DeleteUser(string ftpAccountName);
    Task MoveFolderToRemovedFolders(long folderId);
    Task UpdateFolderPermissions(long folderId);
    void UpdateUser(string ftpAccountName, string password);
}