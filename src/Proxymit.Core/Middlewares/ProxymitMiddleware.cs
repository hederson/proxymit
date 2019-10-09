using Microsoft.AspNetCore.Http;
using Proxymit.Core.Hosts;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace Proxymit.Core.Middleware
{
    public class ProxymitMiddleware
    {
        private static readonly HttpClient _httpClient;
        private readonly RequestDelegate _nextMiddleware;

        public ProxymitMiddleware(RequestDelegate nextMiddleware)
        {
            this._nextMiddleware = nextMiddleware;
        }

        public async Task Invoke(HttpContext httpContext, HostClient hostClient, HostResolver hostResolver)
        {
            var uri = hostResolver.HostUri(httpContext.Request);

            if(uri != null)
            {
                await hostClient.Request(httpContext, uri);
            }

            await _nextMiddleware(httpContext);
        }
    }
}
