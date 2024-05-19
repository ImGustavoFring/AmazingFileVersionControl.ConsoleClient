using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AmazingFileVersionControl.ApiClients;
using AmazingFileVersionControl.Core.DTOs.AuthDTOs;
using AmazingFileVersionControl.Core.DTOs.FileDTOs;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var baseAddress = "http://localhost:5000/";

            using var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };

            var authClient = new AuthClient(httpClient);
            var fileClient = new FileClient(httpClient);

            var loginDto = new LoginDto
            {
                Login = "testuser",
                Password = "TestPassword123"
            };

            var token = await authClient.LoginAsync(loginDto);
            Console.WriteLine("Вход успешен. Токен: " + token);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Путь к файлу R_1.stl
            var filePath = @"C:\Users\David\Downloads\R_1.stl";
            var fileUploadDto = new FileUploadDTO
            {
                Name = "R_1.stl",
                Type = "model/stl",
                Project = "TestProject",
                File = new FormFile(new FileStream(filePath, FileMode.Open), new FileInfo(filePath).Length, Path.GetFileName(filePath))
            };

            var fileId = await fileClient.UploadFileAsync(fileUploadDto);
            Console.WriteLine("Файл успешно загружен. Идентификатор файла: " + fileId);

            var fileQueryDto = new FileQueryDTO
            {
                Name = "R_1.stl",
                Type = "model/stl",
                Project = "TestProject"
            };

            var (stream, fileInfo) = await fileClient.DownloadFileWithMetadataAsync(fileQueryDto);
            var downloadPath = @"C:\Users\David\Downloads\R_1_downloaded.stl";
            using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }
            Console.WriteLine("Файл успешно скачан и сохранен в: " + downloadPath);
            Console.WriteLine("Информация о файле: " + fileInfo.ToJson());
        }
    }

    public class FormFile : IFormFile
    {
        private readonly Stream _stream;

        public FormFile(Stream stream, long length, string fileName)
        {
            _stream = stream;
            Length = length;
            Name = fileName;
            FileName = fileName;
        }

        public string ContentType { get; }
        public string ContentDisposition { get; }
        public IHeaderDictionary Headers { get; }
        public long Length { get; }
        public string Name { get; }
        public string FileName { get; }

        public void CopyTo(Stream target) => _stream.CopyTo(target);
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) => _stream.CopyToAsync(target, cancellationToken);
        public Stream OpenReadStream() => _stream;
    }
}
