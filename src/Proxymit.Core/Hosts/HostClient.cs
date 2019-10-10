using Microsoft.AspNetCore.Http;
using Proxymit.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Proxymit.Core.Hosts
{
    public class HostClient
    {
        private readonly HttpClient httpClient;
        private const int StreamCopyBufferSize = 81920;

        public HostClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        /// <summary>
        /// Make a request to the host and then rewrite entire response's context
        /// </summary>
        /// <param name="httpContext">Current http context</param>
        /// <param name="uriHost">Host destination Uri</param>
        /// <returns></returns>
        public async Task Request(HttpContext httpContext, Uri uriHost)            
        {
            var requestMessage = CreateHostRequestMessage(httpContext, uriHost);            
            using (var responseMessage =
                await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, httpContext.RequestAborted).ConfigureAwait(false))
            {
                await CopyResponse(httpContext, responseMessage).ConfigureAwait(false);
            }
        }

        private HttpRequestMessage CreateHostRequestMessage(HttpContext httpContext, Uri uriHost)
        {
            var requestMessage = new HttpRequestMessage();
            CopyRequestContext(httpContext, requestMessage);

            requestMessage.RequestUri = uriHost;
            requestMessage.Headers.Host = uriHost.Authority;
            
            requestMessage.Method = HttpUtil.GetMethod(httpContext.Request.Method);

            return requestMessage;
        }

        private void CopyRequestContext(HttpContext httpContext, HttpRequestMessage httpRequestMessage)
        {
            var method = HttpUtil.GetMethod(httpContext.Request.Method);
            if(method != HttpMethod.Get &&
                method != HttpMethod.Head &&
                method != HttpMethod.Delete &&
                method != HttpMethod.Trace)
            {
                var streamContent = new StreamContent(httpContext.Request.Body);
                httpRequestMessage.Content = streamContent;
            }

            foreach (var header in httpContext.Request.Headers)
            {
                if (header.Key.StartsWith("X-Forwarded-", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (!httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    httpRequestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            try
            {
                httpRequestMessage.Headers.TryGetValues("User-Agent", out var _);
            }
            catch (IndexOutOfRangeException)
            {
                httpRequestMessage.Headers.Remove("User-Agent");
            }

        }

        private async Task CopyResponse(HttpContext context, HttpResponseMessage responseMessage)
        {
            var response = context.Response;
            response.StatusCode = (int)responseMessage.StatusCode;
            foreach (var header in responseMessage.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach(var header in responseMessage.Content.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            response.Headers.Remove("transfer-encoding");

            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                await responseStream
                    .CopyToAsync(response.Body, StreamCopyBufferSize, context.RequestAborted)
                    .ConfigureAwait(false);
                if (responseStream.CanWrite)
                {
                    await responseStream.FlushAsync(context.RequestAborted).ConfigureAwait(false);
                }
            }
        }
    }
}
