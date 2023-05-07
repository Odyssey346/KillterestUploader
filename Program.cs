using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kurukuru;

namespace KillterestUploader
{
    internal class Program
    {
        private static string? result;
        private static bool failed;

        static void Main(string[] args)
        { 
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: KillterestUploader.exe <path to mp4 file>");
                return;
            }

            var path = args[0];

            if (path.Length == 0) {
                Console.WriteLine("Please submit a path");
                return;
            }

            if (File.Exists(path))
            {
                Spinner.Start("Uploading...", spinner =>
                {
                    var task = Task.Run(() => Upload(path));
                    Task.WaitAll(task);
                    if (failed)
                    {
                        spinner.Fail("Something went wrong! " + result);
                    }
                    else
                    {
                        spinner.Succeed("Uploaded! " + result);
                    }
                });
            } 
            else
            {
                Console.WriteLine("File does not exist");
                return;
            }

            if (Path.GetExtension(path) != ".mp4")
            {
                Console.WriteLine("Please supply an .mp4");
                return;
            }
        }

        static async Task Upload(string file)
        {         
            // generate random number
            Random random = new Random();

            var theRandom = random.Next(1, 1000);

            HttpClient httpClient = new();

            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(File.OpenRead(file));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            content.Add(fileContent, "file", Path.GetFileName(file));

            httpClient.DefaultRequestHeaders.Add("X-UploaderFingerprint", theRandom.ToString());
            httpClient.DefaultRequestHeaders.Add("X-Level", theRandom.ToString());
            httpClient.DefaultRequestHeaders.Add("X-Score", theRandom.ToString());
            httpClient.DefaultRequestHeaders.Add("X-RealTime", theRandom.ToString());
            httpClient.DefaultRequestHeaders.Add("X-GameTime", theRandom.ToString());

            var response = await httpClient.PostAsync(new Uri("https://api.killterest.com/api/Mp4Upload"), content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                result = "Status code: " + response.StatusCode.ToString();
                failed = true;
                return;
            } else
            {
                result = "Link: " + response.Content.ReadAsStringAsync().Result;
                failed = false;
            }
        }
    }
}
