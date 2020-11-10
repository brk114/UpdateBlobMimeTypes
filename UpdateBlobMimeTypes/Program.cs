using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            string CONNECTIONSTRING = "<YOURCONNECTIONSTRING>";
            string CONTAINERNAME = "<YOURCONTAINERNAME>";

            var blobServiceClient = new BlobServiceClient(CONNECTIONSTRING);
            var containerClient = blobServiceClient.GetBlobContainerClient(CONTAINERNAME);

            var FOLDERNAMES = new List<string>() { "video", "image","thumbnail"};
            var messages = new List<string>();
            var unsupportedFiles = new List<string>();
            var errorFiles = new List<string>();

            foreach (var folderName in FOLDERNAMES)
            {
                var blobs = containerClient.GetBlobs(prefix: folderName).ToList();
                Console.WriteLine("connected to folder => " + folderName);
                int i = 0;
                for (; i < blobs.Count; i++)
                {
                    try
                    {
                        var blob = containerClient.GetBlobClient(blobs[i].Name);
                        Console.WriteLine($"Updating {i + 1}=> {blobs[i].Name}");
                        string mimeType = GetContentType(blobs[i].Name);
                        if (!string.IsNullOrEmpty(mimeType))
                        {
                            var blobHttpHeader = new BlobHttpHeaders
                            {
                                ContentType = mimeType
                            };
                            var blobStream = blob.OpenRead();

                            blob.UploadAsync(blobStream, blobHttpHeader).Wait();
                        }
                        else unsupportedFiles.Add($"{CONTAINERNAME}/{blobs[i].Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("============> Error in updating");
                        Console.WriteLine(ex.Message);
                        errorFiles.Add($"{CONTAINERNAME}/{blobs[i].Name} => {ex.Message}");

                        Console.WriteLine(new string('=', 20));
                    }
                }
                messages.Add($"{CONTAINERNAME}/{folderName} => {i}");
            }
            var lineSperator = new string('=', 40);
            Console.WriteLine(lineSperator);
            messages.ForEach(x => Console.WriteLine(x));
            
            Console.WriteLine(lineSperator);
            unsupportedFiles.ForEach(x => Console.WriteLine(x));
            Console.WriteLine(lineSperator);
            errorFiles.ForEach(x => Console.WriteLine(x));
        }

        private static string GetContentType(string name)
        {
            var typesList = new Dictionary<string, string>() {
                //image formats
                {".jpg"  ,"image/jpeg"},
                {".jpeg" ,"image/jpeg"},
                {".jpe"  ,"image/jpeg"},
                {".gif"  ,"image/gif"},
                {".png"  ,"image/png"},
                {".svg"  ,"image/svg+xml"},
                {".webp" ,"image/webp"},
                {".tiff" ,"image/tiff"},
                {".bmp" ,"image/bmp"},
                //video formats
                {".mp4"  ,"video/mp4"},
                {".avi"  ,"video/x-msvideo"},
                {".wmv"  ,"video/x-ms-wmv"},
                {".mov"  ,"video/quicktime"},
                {".flv"  ,"video/x-flv"},
                {".webm" ,"video/webm"},
                // file formats
                {".txt"  ,"text/plain"},
                {".pdf"  ,"application/pdf"},
                {".csv"  ,"application/csv"},
                {".rtf"  ,"application/vnd.rtf"},
                {".doc"  ,"application/msword"},
                {".xls"  ,"application/vnd.ms-excel"},
                {".ppt"  ,"application/vnd.vnd.ms-powerpoint"},
                {".docx" ,"application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xlsx" ,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".pptx" ,"application/vnd.openxmlformats-officedocument.presentationml.presentation"},
                {".wpd"   ,"application/vnd.wordperfect"},
                // archive formats
                {".zip"  ,"application/zip"},
                {".rar"  ,"application/vnd.rar"},
                {".7z"   ,"application/x-7z-compressed"}
            };
            var ext = Path.GetExtension(name);
            if (!typesList.TryGetValue(ext, out string mimetype))
                mimetype = "";

            return mimetype;
        }
    }
}
