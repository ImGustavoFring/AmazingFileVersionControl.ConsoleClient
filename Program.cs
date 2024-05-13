using AmazingFileVersionControl.ApiClients;
using AmazingFileVersionControl.Core.DTOs.FileDTOs;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AmazingFileVersionControl.ApiClients.ApiClients;
using AmazingFileVersionControl.ApiClients.Helpers;
using AmazingFileVersionControl.Core.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Http.Internal;

namespace AmazingFileVersionControl.ConsoleClient
{
    public class TestFileController
    {
        private readonly FileApiClient _fileClient;

        public TestFileController(FileApiClient fileClient)
        {
            _fileClient = fileClient;
        }

        static async Task Main(string[] args)
        {
            var authBaseUrl = "http://localhost:5000/api/UserAuth";
            var fileBaseUrl = "http://localhost:5000/api/file";

            var authClient = new AuthApiClient(authBaseUrl);
            var fileClient = new FileApiClient(fileBaseUrl);

            string user1Token = await RegisterAndLoginUser(authClient, "user1", "user1@example.com", "securepassword");
            await Console.Out.WriteLineAsync($"User1 token: {user1Token}");

            var testFileController = new TestFileController(fileClient);

            await testFileController.RunFileTests(user1Token);
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
                await Console.Out.WriteLineAsync($"Error registering or logging in {login}: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task RunFileTests(string userToken)
        {
            _fileClient.SetToken(userToken);

            // Upload a file
            await UploadFile("user1", "testfile.txt", "project1", "This is a test file content");

            // Fetch file info
            await GetFileInfo("testfile.txt", "user1", "project1", -1);

            // Update file info
            await UpdateFileInfo("testfile.txt", "user1", "project1", 1, "{ \"newKey\": \"newValue\" }");

            // Update all owner files info
            await UpdateAllOwnerFilesInfo("user1", "{ \"updatedKey\": \"updatedValue\" }");

            // Download the file
            await DownloadFile("testfile.txt", "user1", "project1", 1);

            // Delete the file
            await DeleteFile("testfile.txt", "user1", "project1", 1);

            // Delete all files for the user
            await DeleteAllOwnerFiles("user1");
        }

        private async Task UploadFile(string owner, string fileName, string project, string fileContent)
        {
            try
            {
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
                var result = await _fileClient.UploadOwnerFileAsync(fileUploadDTO);
                await Console.Out.WriteLineAsync($"{owner} uploaded {fileName}: {result}");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error uploading file {fileName} for {owner}: {ex.Message}");
            }
        }

        private async Task GetFileInfo(string fileName, string owner, string project, int version)
        {
            try
            {
                var fileQueryDTO = new FileQueryDTO
                {
                    Name = fileName,
                    Owner = owner,
                    Project = project,
                    Version = version
                };
                var info = await _fileClient.GetOwnerFileInfoAsync(fileQueryDTO);
                await Console.Out.WriteLineAsync($"{owner}'s {fileName} info: {info}");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error getting info for file {fileName} of {owner}: {ex.Message}");
            }
        }

        private async Task UpdateFileInfo(string fileName, string owner, string project, int version, string updatedMetadataJson)
        {
            try
            {
                var fileUpdateDTO = new FileUpdateDTO
                {
                    Name = fileName,
                    Owner = owner,
                    Project = project,
                    Version = version,
                    UpdatedMetadata = updatedMetadataJson
                };
                await _fileClient.UpdateOwnerFileInfoAsync(fileUpdateDTO);
                await Console.Out.WriteLineAsync($"Updated file {fileName} of {owner}");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error updating file {fileName} of {owner}: {ex.Message}");
            }
        }

        private async Task UpdateAllOwnerFilesInfo(string owner, string updatedMetadataJson)
        {
            try
            {
                await _fileClient.UpdateOwnerAllFilesInfoAsync(owner, updatedMetadataJson);
                await Console.Out.WriteLineAsync($"Updated all files info for {owner}");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error updating all files info for {owner}: {ex.Message}");
            }
        }

        private async Task DownloadFile(string fileName, string owner, string project, int version)
        {
            try
            {
                var fileQueryDTO = new FileQueryDTO
                {
                    Name = fileName,
                    Owner = owner,
                    Project = project,
                    Version = version
                };
                var stream = await _fileClient.DownloadOwnerFileAsync(fileQueryDTO);
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();
                await Console.Out.WriteLineAsync($"{owner}'s {fileName} content: {content}");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error downloading file {fileName} of {owner}: {ex.Message}");
            }
        }

        private async Task DeleteFile(string fileName, string owner, string project, int version)
        {
            try
            {
                var fileQueryDTO = new FileQueryDTO
                {
                    Name = fileName,
                    Owner = owner,
                    Project = project,
                    Version = version
                };
                await _fileClient.DeleteOwnerFileAsync(fileQueryDTO);
                await Console.Out.WriteLineAsync($"{owner}'s file {fileName} deleted");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error deleting file {fileName} of {owner}: {ex.Message}");
            }
        }

        private async Task DeleteAllOwnerFiles(string owner)
        {
            try
            {
                await _fileClient.DeleteOwnerAllFilesAsync(owner);
                await Console.Out.WriteLineAsync($"Deleted all files for {owner}");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Error deleting all files for {owner}: {ex.Message}");
            }
        }
    }
}
