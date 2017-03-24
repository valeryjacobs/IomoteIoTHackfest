using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace speech_01
{
    public static class HTTPHelper
    {
        public static async Task<string> Send(HttpMethod method, string url)
        {
            var client = new HttpClient();
            var msg = new HttpRequestMessage(method, url);
            //msg.Content = new StringContent(body);
            var response = await client.SendAsync(msg);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();
            else
                return response.StatusCode.ToString();
        }
    }
}
