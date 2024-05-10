using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AmazingFileVersionControl.ApiClients.ApiClients;
using AmazingFileVersionControl.ApiClients.Helpers;
using AmazingFileVersionControl.Core.DTOs.AuthDTOs;
using AmazingFileVersionControl.Core.DTOs.FileDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace AmazingFileVersionControl.ConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var authBaseUrl = "http://localhost:5000/api/UserAuth";
            var fileBaseUrl = "http://localhost:5000/api/file";
            var authClient = new AuthApiClient(authBaseUrl);
            var fileClient = new FileApiClient(fileBaseUrl);

            // Register and login User1
            string user1Token = await RegisterAndLoginUser(authClient, "user1", "user1@example.com", "securepassword");
            Console.WriteLine($"User1 token: {user1Token}");

            // Register and login User2
            string user2Token = await RegisterAndLoginUser(authClient, "user2", "user2@example.com", "securepassword");
            Console.WriteLine($"User2 token: {user2Token}");

            // User1 uploads a file
            await UploadFile(fileClient, user1Token, "user1", "file1.txt", "project1", "User1's file content");

            // User2 uploads a file
            await UploadFile(fileClient, user2Token, "user2", "file2.txt", "project2", "User2's file content");

            // User1 attempts to update User2's file
            await UpdateFileInfo(fileClient, user1Token, "file2.txt", "user2", "project2", 1, "{ \"newKey\": \"newValue\" }");

            // User2 attempts to download User1's file
            await DownloadFile(fileClient, user2Token, "file1.txt", "user1", "project1", 1);

            // Fetch file info
            await GetFileInfo(fileClient, user1Token, "file1.txt", "user1", "project1", -1);
            await GetFileInfo(fileClient, user2Token, "file2.txt", "user2", "project2", 1);

            // Update all owner files info
            await UpdateAllOwnerFilesInfo(fileClient, user1Token, "user1", "{ \"updatedKey\": \"updatedValue\" }");

            // Delete a file
            await DeleteFile(fileClient, user2Token, "file2.txt", "user2", "project2", 1);

            // Delete all files for user1
            await DeleteAllOwnerFiles(fileClient, user1Token, "user1");
           
        }

        private static async Task<string> RegisterAndLoginUser(AuthApiClient authClient, string login, string email, string password)
        {
            try
            {
                var registerDTO = new RegisterDTO
                {
                    Login = login,
                    Email = email,
                    Password = password
                };
                await authClient.RegisterAsync(registerDTO);
                var loginDTO = new LoginDTO
                {
                    LoginOrEmail = login,
                    Password = password
                };
                var loginResponse = await authClient.LoginAsync(loginDTO);
                return TokenHelper.ExtractToken(loginResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering or logging in {login}: {ex.Message}");
                return string.Empty;
            }
        }

        private static async Task UploadFile(FileApiClient fileClient, string token, string owner, string fileName, string project, string fileContent)
        {
            try
            {
                fileClient.SetToken(token);
                var fileUploadDTO = new FileUploadDTO
                {
                    Name = fileName,
                    Owner = owner,
                    Project = project,
                    Type = "text/plain",
                    Description = $"{owner}'s file",
                    File = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(fileContent)),
                        0, fileContent.Length, "File", fileName)
                };
                var result = await fileClient.UploadFileAsync(fileUploadDTO);
                Console.WriteLine($"{owner} uploaded {fileName}: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file {fileName} for {owner}: {ex.Message}");
            }
        }

        private static async Task UpdateFileInfo(FileApiClient fileClient, string token, string fileName, string owner, string project, int version, string updatedMetadataJson)
        {
            try
            {
                fileClient.SetToken(token);
                var fileUpdateDTO = new FileUpdateDTO
                {
                    Name = fileName,
                    Owner = owner,
                    Project = project,
                    Version = version,
                    UpdatedMetadata = updatedMetadataJson
                };
                await fileClient.UpdateFileInfoAsync(fileUpdateDTO);
                Console.WriteLine($"{token} updated file {fileName} of {owner}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating file {fileName} of {owner}: {ex.Message}");
            }
        }

        private static async Task DownloadFile(FileApiClient fileClient, string token, string fileName, string owner, string project, int version)
        {
            try
            {
                fileClient.SetToken(token);
                var fileQueryDTO = new FileQueryDTO
                {
                    Name = fileName,
                    Owner = owner,
                    Project = project,
                    Version = version
                };
                var stream = await fileClient.DownloadFileAsync(fileQueryDTO);
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();
                Console.WriteLine($"{owner}'s {fileName} content: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file {fileName} of {owner}: {ex.Message}");
            }
        }

        private static async Task GetFileInfo(FileApiClient fileClient, string token, string fileName, string owner, string project, int version)
        {
            try
            {
                fileClient.SetToken(token);
                var fileQueryDTO = new FileQueryDTO
                {
                    Name = fileName,
                    Owner = owner,
                    Project = project,
                    Version = version
                };
                var info = await fileClient.GetFileInfoAsync(fileQueryDTO);
                Console.WriteLine($"{owner}'s {fileName} info: {info}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting info for file {fileName} of {owner}: {ex.Message}");
            }
        }

        private static async Task UpdateAllOwnerFilesInfo(FileApiClient fileClient, string token, string owner, string updatedMetadataJson)
        {
            try
            {
                fileClient.SetToken(token);
                await fileClient.UpdateAllOwnerFilesInfoAsync(owner, updatedMetadataJson);
                Console.WriteLine($"Updated all files info for {owner}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating all files info for {owner}: {ex.Message}");
            }
        }

        private static async Task DeleteFile(FileApiClient fileClient, string token, string fileName, string owner, string project, int version)
        {
            try
            {
                fileClient.SetToken(token);
                var fileQueryDTO = new FileQueryDTO
                {
                    Name = fileName,
                    Owner = owner,
                    Project = project,
                    Version = version
                };
                await fileClient.DeleteFileAsync(fileQueryDTO);
                Console.WriteLine($"{owner}'s file {fileName} deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file {fileName} of {owner}: {ex.Message}");
            }
        }

        private static async Task DeleteAllOwnerFiles(FileApiClient fileClient, string token, string owner)
        {
            try
            {
                fileClient.SetToken(token);
                await fileClient.DeleteAllOwnerFilesAsync(owner);
                Console.WriteLine($"Deleted all files for {owner}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting all files for {owner}: {ex.Message}");
            }
        }
    }
}
