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
        public ProxymitMiddleware(RequestDelegate _)
        {
            
        }

        public async Task Invoke(HttpContext httpContext, HostClient hostClient)
        {
            await hostClient.ReverseProxy(httpContext).ConfigureAwait(false);
        }
    }
}
