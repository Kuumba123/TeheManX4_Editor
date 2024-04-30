using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace TeheManX4
{
    static class Redux
    {
        #region Fields
        static private HttpClient client = new HttpClient(new HttpClientHandler() { UseProxy = false });
        #endregion Fields

        #region Methods
        static async internal Task Pause()
        {
            HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:" + MainWindow.settings.webPort + "/api/v1/execution-flow?function=pause", null);
            response.EnsureSuccessStatusCode();
        }
        static async internal Task Resume()
        {
            HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:" + MainWindow.settings.webPort + "/api/v1/execution-flow?function=resume", null);
            response.EnsureSuccessStatusCode();
        }
        static async internal Task Write(uint offset, byte val)
        {
            offset &= 0x1FFFFFFF;
            string url = string.Format("http://127.0.0.1:" + MainWindow.settings.webPort + "/api/v1/cpu/ram/raw?offset={0}&size=1", offset);
            MultipartFormDataContent content = new MultipartFormDataContent();

            StreamContent fileStreamContent = new StreamContent(new MemoryStream(new byte[] { val }));
            content.Add(fileStreamContent);
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        static async internal Task Write(uint offset, byte[] vals)
        {
            offset &= 0x1FFFFFFF;
            string url = string.Format(@"http://127.0.0.1:" + MainWindow.settings.webPort + "/api/v1/cpu/ram/raw?offset={0}&size={1}", offset, vals.Length);
            MultipartFormDataContent content = new MultipartFormDataContent();


            StreamContent fileStreamContent = new StreamContent(new MemoryStream(vals));
            content.Add(fileStreamContent);
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        static async internal Task Write(uint offset, ushort val)
        {
            offset &= 0x1FFFFFFF;
            string url = string.Format(@"http://127.0.0.1:" + MainWindow.settings.webPort + "/api/v1/cpu/ram/raw?offset={0}&size=2", offset);
            MultipartFormDataContent content = new MultipartFormDataContent();


            StreamContent fileStreamContent = new StreamContent(new MemoryStream(BitConverter.GetBytes(val)));
            content.Add(fileStreamContent);
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        static async internal Task Write(uint offset, int val)
        {
            offset &= 0x1FFFFFFF;
            string url = string.Format(@"http://127.0.0.1:" + MainWindow.settings.webPort + "/api/v1/cpu/ram/raw?offset={0}&size=4", offset);
            MultipartFormDataContent content = new MultipartFormDataContent();


            StreamContent fileStreamContent = new StreamContent(new MemoryStream(BitConverter.GetBytes(val)));
            content.Add(fileStreamContent);
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        static async internal Task DrawRect(Int32Rect rect, byte[] data)
        {
            string url = string.Format(@"http://127.0.0.1:" + MainWindow.settings.webPort + "/api/v1/gpu/vram/raw?x={0}&y={1}&width={2}&height={3}", rect.X, rect.Y, rect.Width, rect.Height);
            MultipartFormDataContent content = new MultipartFormDataContent();

            StreamContent fileStreamContent = new StreamContent(new MemoryStream(data));
            content.Add(fileStreamContent);
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        #endregion Methods
    }
}
