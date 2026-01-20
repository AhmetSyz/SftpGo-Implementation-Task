using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // For IConfiguration
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics.Tracing;
using System.Threading.Tasks.Sources;
using System.Transactions;
using System.Runtime.InteropServices.Marshalling;
using System.Reflection.PortableExecutable; // For JsonConvert
namespace SftpGo.Service;

public class SftpGoService : IFileTransfer
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private string? _token;
    public SftpGoService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }
    // This method gets a fresh token from the /token endpoint
    private async Task RefreshTokenAsync()
    {
        var adminUser = _config["SFTPGoSettings:AdminUsername"];
        var adminPass = _config["SFTPGoSettings:AdminPassword"];
        // SFTPGo uses Basic Auth on the /token endpoint to give you a JWT
        var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{adminUser}:{adminPass}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
        var response = await _httpClient.GetAsync($"{_config["SFTPGoSettings:BaseUrl"]}/token");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(json);
        _token = result?.access_token?.ToString();
    }
    public async Task CreateUserAsync(string username, string password)
    {
        await RefreshTokenAsync(); // Always get a fresh token first
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token!);
        var newUser = new
        {
            username = username,
            password = password,
            status = 1,
            home_dir = $"{_config["SFTPGoSettings:InternalDataRoot"]}/{username}",
            permissions = new Dictionary<string, string[]> { { "/", new[] { "*" } } }
        };
        var content = new StringContent(JsonConvert.SerializeObject(newUser), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_config["SFTPGoSettings:BaseUrl"]}/users", content);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"SFTPGo Error: {error}");
        }
    }
    public async Task DeleteUserAsync(string username, bool deleteData = true)
    {
        await RefreshTokenAsync(); // Get fresh token
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        // query parameter delete_data=1 ensures the physical Windows folder is also deleted
        int deleteDataFlag = deleteData ? 1 : 0;
        var url = $"{_config["SFTPGoSettings:BaseUrl"]}/users/{username}?delete_data={deleteDataFlag}";
        var response = await _httpClient.DeleteAsync(url);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[SFTPGo] User '{username}' deleted successfully.");
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Console.WriteLine($"[SFTPGo] User '{username}' was not found (already deleted).");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"SFTPGo Delete Error ({response.StatusCode}): {error}");
        }
    }
    // You should implement all of the above interface methods here.
    // You may check my methods to check how to implement them.
    public void UpdateUser(string ftpAccountName, string password)
    {
        throw new NotImplementedException();
    }
    public Task AddUser(string ftpAccountName, string password, long projectId)
    {
        throw new NotImplementedException();
    }
    public void ActivateUsers(List<string> usernames)
    {
        throw new NotImplementedException();
    }
    public void DeactivateUsers(List<string> usernames)
    {
        throw new NotImplementedException();
    }
    public void DeleteUser(string ftpAccountName)
    {
        throw new NotImplementedException();
    }
    public Task CreateFolder(long folderId)
    {
        throw new NotImplementedException();
    }
    public Task MoveFolderToRemovedFolders(long folderId)
    {
        throw new NotImplementedException();
    }
    public Task UpdateFolderPermissions(long folderId)
    {
        throw new NotImplementedException();
    }

}
