using Microsoft.AspNetCore.Http;
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
        private readonly RequestDelegate requestDelegate;

        public ProxymitMiddleware(RequestDelegate requestDelegate)
        {
            this.requestDelegate = requestDelegate;
        }

        public async Task Invoke(HttpContext httpContext, IHttpClientFactory httpClientFactory)
        {
            

            await requestDelegate(httpContext);
        }
    }
}
