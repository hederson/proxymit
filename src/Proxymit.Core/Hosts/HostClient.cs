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
                await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, httpContext.RequestAborted))
            {
                httpContext.Response.StatusCode = (int)responseMessage.StatusCode;
                
                CopyResponseHeaders(httpContext, responseMessage);
                await ProcessResponse(httpContext, responseMessage);                
            }
        }

        private async Task ProcessResponse(HttpContext context, HttpResponseMessage  responseMessage)
        {
            await context.Response.Body.WriteAsync(await responseMessage.Content.ReadAsByteArrayAsync());
        }

        private HttpRequestMessage CreateHostRequestMessage(HttpContext httpContext, Uri uriHost)
        {
            var requestMessage = new HttpRequestMessage();
            CopyRequestContext(httpContext, requestMessage);

            requestMessage.RequestUri = uriHost;
            requestMessage.Headers.Host = uriHost.Host;
            requestMessage.Headers.TransferEncodingChunked = false;
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

            foreach(var header in httpContext.Request.Headers)
            {
                httpRequestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private void CopyResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach(var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach(var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            context.Response.Headers.Remove("transfer-encoding");
            context.Response.Headers.Remove("Transfer-Encoding");
        }
    }
}
