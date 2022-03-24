using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CustomBeatmaps.Util
{
    /// <summary>
    /// Simple means to asynchronously fetch/get server queries
    /// </summary>
    public static class ServerHelper
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public static async Task<T> GetJSON<T>(string url)
        {
            var response = await HttpClient.GetAsync(url);

            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                return await SerializeHelper.DeserializeJSONAsync<T>(await response.Content.ReadAsStreamAsync());
            }
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        public static async Task<T> PostJSON<T>(string url, object data)
        {
            HttpContent content = new StringContent(SerializeHelper.SerializeJSON(data));
            var response = await HttpClient.PostAsync(url, content);

            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                return await SerializeHelper.DeserializeJSONAsync<T>(await response.Content.ReadAsStreamAsync());
            }
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        public static async Task DownloadFile(string url, string downloadPath)
        {
            var stream = await HttpClient.GetStreamAsync(url);

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
        }
    }
}
