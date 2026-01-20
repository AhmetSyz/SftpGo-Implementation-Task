using Microsoft.Extensions.Configuration;
using SftpGo.Service;

Console.WriteLine("--- SFTPGo Service Test ---");

// 1. Build Configuration to read appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// 2. Initialize HttpClient and the Service
using var httpClient = new HttpClient();
var sftpGoService = new SftpGoService(httpClient, configuration);

try
{
    Console.WriteLine("Attempting to create user...");
    
    // 3. Run the test
    string testUser = "ouzayb";
    string testPass = "ouzayb!";
    
    await sftpGoService.CreateUserAsync(testUser, testPass);
    
    Console.WriteLine($"Success! User '{testUser}' created successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Details: {ex.InnerException.Message}");
    }
}