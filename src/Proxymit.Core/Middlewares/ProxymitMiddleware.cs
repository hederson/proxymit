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
        private readonly RequestDelegate _next;

        public ProxymitMiddleware(RequestDelegate _next)
        {
            this._next = _next;
        }

        public async Task Invoke(HttpContext httpContext, HostClient hostClient)
        {
            var success = await hostClient.ReverseProxy(httpContext).ConfigureAwait(false);
            if(!success)
            {
                await _next(httpContext);
            }
        }
    }
}
