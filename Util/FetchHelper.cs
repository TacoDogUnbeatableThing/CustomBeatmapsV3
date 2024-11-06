using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using File = Pri.LongPath.File;
using Path = Pri.LongPath.Path;
using Directory = Pri.LongPath.Directory;

namespace CustomBeatmaps.Util
{
    /// <summary>
    /// Simple means to asynchronously fetch/get server queries
    /// </summary>
    public static class FetchHelper
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public static async Task<T> GetJSON<T>(string url, Dictionary<string, string> headers = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (headers != null)
            {
                foreach(var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            var response = await HttpClient.SendAsync(request);

            string mediaType = response.Content.Headers.ContentType?.MediaType;
            if (mediaType == "application/json")
            {
                return await SerializeHelper.DeserializeJSONAsync<T>(await response.Content.ReadAsStreamAsync());
            }
            string error = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(error))
            {
                // emptyy
                return default;
            }
            throw new HttpRequestException(error);
        }

        public static async Task<T> PostJSON<T>(string url, object data)
        {
            string serialized = SerializeHelper.SerializeJSON(data);
            HttpContent content = new StringContent(serialized, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(url, content);

            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                return await SerializeHelper.DeserializeJSONAsync<T>(await response.Content.ReadAsStreamAsync());
            }
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        public static async Task<bool> GetAvailable(string url)
        {
            var response = await HttpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        public static async Task DownloadFile(string url, string downloadPath)
        {
            var response = await HttpClient.GetAsync(url);

            using (var fs = new FileStream(downloadPath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }

            /*
            // Define buffer and buffer size
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
 
            // Read from response and write to file
            FileStream fileStream = File.Create(downloadPath);
            while (stream != null && (bytesRead = await stream.ReadAsync(buffer, 0, bufferSize)) != 0) 
            {
                fileStream.Write(buffer, 0, bytesRead);
            } // end while
            */
        }
    }
}
