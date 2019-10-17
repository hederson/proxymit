using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Proxymit.Core.Configs;
using Proxymit.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Proxymit.Core.Hosts
{
    public class HostClient
    {
        private readonly HttpClient httpClient;
        private readonly HostResolver hostResolver;
        private readonly IOptions<ExposedPortConfig> settings;
        private const int StreamCopyBufferSize = 81920;

        public HostClient(HttpClient httpClient, HostResolver hostResolver, IOptions<ExposedPortConfig> settings)
        {
            this.httpClient = httpClient;
            this.hostResolver = hostResolver;
            this.settings = settings;
        }

        public async Task<bool> ReverseProxy(HttpContext context)
        {
            var destinationConfig = hostResolver.GetMathingCofinguration(context.Request);            
            if (destinationConfig != null)
            {
                
                if (destinationConfig.HttpsRedirect && !context.Request.IsHttps)
                {

                    context.Response.Redirect(new UriBuilder("https",
                        context.Request.Host.Host, settings.Value.Https, context.Request.Path.Value).ToString());

                    return true;

                }              

                var uri = hostResolver.GetHostUri(context.Request, destinationConfig);
                await Request(context, uri).ConfigureAwait(false);
                return true;
                
            }
            else
            {
                return false;
            }
        }

        private async Task BadRequest(HttpContext context)
        {
            var badRequestMessage = Encoding.UTF8.GetBytes("400 - Bad Request");
            context.Response.StatusCode = 400;
            
            try
            {
                context.Response.Headers.TryGetValue("User-Agent", out var _);
            }
            catch (IndexOutOfRangeException)
            {
                context.Response.Headers.Remove("User-Agent");
            }

            using (var stream = new MemoryStream(badRequestMessage))
            {
                stream.Position = 0;
                await stream.CopyToAsync(context.Response.Body, StreamCopyBufferSize, context.RequestAborted)
                    .ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Make a request to the host and then rewrite entire response's context
        /// </summary>
        /// <param name="httpContext">Current http context</param>
        /// <param name="uriHost">Host destination Uri</param>
        /// <returns></returns>
        private async Task Request(HttpContext httpContext, Uri uriHost)            
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
