using System;
using System.Text;
using System.Threading.Tasks;
using AmazingFileVersionControl.ApiClients.ApiClients;
using AmazingFileVersionControl.Core.DTOs.AuthDTOs;
using AmazingFileVersionControl.Core.DTOs.FileDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using MongoDB.Bson;

namespace AmazingFileVersionControl.ConsoleClient
{
    //class Program
    //{
    //    static async Task Main(string[] args)
    //    {
    //        var baseUrl = "https://localhost:7000/api/UserAuth";

    //        var handler = new HttpClientHandler
    //        {
    //            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    //        };

    //        var authApiClient = new AuthApiClient(baseUrl);

    //        // Register a new user
    //        try
    //        {
    //            var registerRequest = new RegisterDTO
    //            {
    //                Login = "example_user1",
    //                Email = "user1@example.com",
    //                Password = "securepassword"
    //            };

    //            var registerResult = await authApiClient.RegisterAsync(registerRequest);
    //            await Console.Out.WriteLineAsync($"Registration successful. Token: {registerResult}");
    //        }
    //        catch (Exception ex)
    //        {
    //            await Console.Out.WriteLineAsync($"Registration failed. Error: {ex.Message}");
    //        }

    //        // Login
    //        try
    //        {
    //            var loginRequest = new LoginDTO
    //            {
    //                LoginOrEmail = "example_user1",
    //                Password = "securepassword"
    //            };

    //            var loginResult = await authApiClient.LoginAsync(loginRequest);
    //            await Console.Out.WriteLineAsync($"Login successful. Token: {loginResult}");
    //        }
    //        catch (Exception ex)
    //        {
    //            await Console.Out.WriteLineAsync($"Login failed. Error: {ex.Message}");
    //        }
    //        await Console.Out.WriteLineAsync();
    //    }
    //}

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var baseUrl = "http://localhost:5000/api/file";
            var client = new FileApiClient(baseUrl);

            // Upload a file
            var fileUploadDto = new FileUploadDTO
            {
                Name = "example.txt",
                Owner = "user1",
                Project = "project1",
                Type = "text/plain",
                Description = "Sample file upload",
                File = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("Sample file content")), 0, "Sample file content".Length, "File", "example.txt")
            };
            var uploadResult = await client.UploadFileAsync(fileUploadDto);
            Console.WriteLine($"Upload Result: {uploadResult}");

            // Download a file
            var fileQueryDto = new FileQueryDTO
            {
                Name = "example.txt",
                Owner = "user1",
                Project = "project1",
                Version = 1
            };
            var fileStream = await client.DownloadFileAsync(fileQueryDto);
            using var reader = new StreamReader(fileStream);
            var fileContent = await reader.ReadToEndAsync();

            await Console.Out.WriteLineAsync($"Downloaded File Content: {fileContent}");

            // Get file info
            var fileInfo = await client.GetFileInfoAsync(fileQueryDto);
            await Console.Out.WriteLineAsync($"File Info: {fileInfo}");

            // Get all files info for owner
            var allFilesInfo = await client.GetAllOwnerFilesInfoAsync("user1");
            await Console.Out.WriteLineAsync($"All Files Info: {allFilesInfo}");

            // Update file info
            var fileUpdateDto = new FileUpdateDTO
            {
                Name = "example.txt",
                Owner = "user1",
                Project = "project1",
                Version = 1,
                UpdatedMetadata = new BsonDocument { { "newKey", "newValue" } }.ToJson()
            };
            await client.UpdateFileInfoAsync(fileUpdateDto);
            await Console.Out.WriteLineAsync("File info updated.");

            // Update all files info for owner
            var updateAllFilesDto = new UpdateAllFilesDTO
            {
                Owner = "user1",
                UpdatedMetadata = new BsonDocument { { "globalKey", "globalValue" } }.ToJson()
            };
            await client.UpdateAllOwnerFilesInfoAsync(updateAllFilesDto.Owner, updateAllFilesDto.UpdatedMetadata);
            await Console.Out.WriteLineAsync("All files info updated.");

            // Delete file
            await client.DeleteFileAsync(fileQueryDto);
            await Console.Out.WriteLineAsync("File deleted.");

            // Delete all files for owner
            await client.DeleteAllOwnerFilesAsync("user1");
            await Console.Out.WriteLineAsync("File deleted.");
        }
    }

}
