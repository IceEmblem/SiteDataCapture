using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler.Models
{
    public class RequestData:IDisposable
    {
        public string UserAgent { get; set; }

        public CookieContainer CookieContainer { get; set; }

        public int Timeout { get; set; }

        private string url = null;
        private HttpWebRequest request = null;
        private HttpWebResponse response = null;
        private MemoryStream memoryStream = null;

        public RequestData(string url)
        {
            this.url = url;

            request = WebRequest.Create(url) as HttpWebRequest;
        }

        public RequestResult Request()
        {
            // 创建并配置Web请求
            request = WebRequest.Create(url) as HttpWebRequest;

            if (request == null) {
                return null;
            }

            ConfigRequest(request);

            try
            {
                response = request.GetResponseAsync().Result as HttpWebResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }


            if (response == null)
            {
                return null;
            }

            PersistenceCookie(response);

            Stream responsestream = response.GetResponseStream();

            if (responsestream == null)
            {
                return null;
            }

            // 如果页面压缩，则解压数据流
            if (response.ContentEncoding == "gzip")
            {
                responsestream = new GZipStream(responsestream, CompressionMode.Decompress);
            }

            memoryStream = new MemoryStream();
            responsestream.CopyTo(memoryStream);

            string html = ParseContent(memoryStream, response.CharacterSet);

            return new RequestResult() { Html = html, Stream = memoryStream };
        }

        /// <summary>
        /// 配置请求
        /// </summary>
        private void ConfigRequest(HttpWebRequest request)
        {
            request.AllowAutoRedirect = true;
            request.MediaType = "text/html";
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8";

            request.UserAgent = this.UserAgent;
            request.CookieContainer = this.CookieContainer;
            if (this.Timeout > 0)
            {
                request.Timeout = this.Timeout;
            }
        }

        /// <summary>
        /// 设置cookie
        /// </summary>
        /// <param name="response"></param>
        private void PersistenceCookie(HttpWebResponse response)
        {
            if (this.CookieContainer == null) {
                return;
            }

            string cookies = response.Headers["Set-Cookie"];
            if (!string.IsNullOrEmpty(cookies))
            {
                var cookieUri =
                    new Uri(
                        string.Format(
                            "{0}://{1}:{2}/",
                            response.ResponseUri.Scheme,
                            response.ResponseUri.Host,
                            response.ResponseUri.Port));

                this.CookieContainer.SetCookies(cookieUri, cookies);
            }
        }

        /// <summary>
        /// 解析接受的html
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="characterSet"></param>
        /// <returns></returns>
        private string ParseContent(MemoryStream stream, string characterSet)
        {
            byte[] buffer = stream.ToArray();

            Encoding encode = Encoding.ASCII;
            string html = encode.GetString(buffer);

            string localCharacterSet = characterSet;

            Match match = Regex.Match(html, "<meta([^<]*)charset=([^<]*)\"", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                localCharacterSet = match.Groups[2].Value;

                var stringBuilder = new StringBuilder();
                foreach (char item in localCharacterSet)
                {
                    if (item == ' ')
                    {
                        break;
                    }

                    if (item != '\"')
                    {
                        stringBuilder.Append(item);
                    }
                }

                localCharacterSet = stringBuilder.ToString();
            }

            if (string.IsNullOrEmpty(localCharacterSet))
            {
                localCharacterSet = characterSet;
            }

            if (!string.IsNullOrEmpty(localCharacterSet))
            {
                encode = Encoding.GetEncoding(localCharacterSet);
            }

            return encode.GetString(buffer);
        }

        public void Dispose()
        {
            if (request != null)
            {
                request.Abort();
            }

            if (response != null)
            {
                response.Close();
            }

            if (memoryStream != null)
            {
                memoryStream.Close();
            }
        }
    }
}
