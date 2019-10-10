using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Proxymit.Core.Configs;
using Proxymit.Core.Hosts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proxymit.Tests.Hosts
{
    [TestFixture]
    public class HostClientTests
    {
        private HttpContext httpContext;
        private Mock<HttpMessageHandler> messageHandler;
        private Mock<HostResolver> hostResolver;
        private HeaderDictionary headerDictionary;
        private HttpClient httpClient;
        private IOptions<ExposedPortConfig> exposedConfig;
        private string responseBodyString = "response works";

        [SetUp]
        public void Init()
        {   

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            responseMessage.Headers.TryAddWithoutValidation("CustomHeader", "1");
            responseMessage.Content = new StringContent(responseBodyString);            
            responseMessage.Content.Headers.TryAddWithoutValidation("CustomHeader2", "2");

            messageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            messageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            httpClient = new HttpClient(messageHandler.Object);

            hostResolver = new Mock<HostResolver>(new Mock<IDomainConfigurationLoader>().Object);

            exposedConfig = Options.Create(new ExposedPortConfig
            {
                Http = 5002,
                Https = 5003
            });

        }

        private void SetRequestMethod(string httpMethod)
        {
            httpContext = new DefaultHttpContext();

            var bodyStream = new MemoryStream(Encoding.ASCII.GetBytes("it's work"));
            bodyStream.Flush();
            bodyStream.Position = 0;

            httpContext.Request.Body = bodyStream;
            httpContext.Request.Method = httpMethod;

            httpContext.Request.Headers.TryAdd("Content-Type", "text/plain");
            httpContext.Request.Headers.TryAdd("Authentication", "Bearer 123csharp");

            hostResolver.Setup(x => x.GetMathingCofinguration(httpContext.Request))
                .Returns(new DomainConfiguration
                {
                    
                });

            
            httpContext.Response.Body = new MemoryStream();
        }

        [Test]
        [TestCase("http://test.com", "GET", false)]
        [TestCase("http://test.com/api", "POST", false)]
        public async Task Should_RewriteResponseContext(string url, string method, bool redirectHttps)
        {
            var domainConfiguration = new DomainConfiguration
            {
                HttpsRedirect = redirectHttps
            };

            hostResolver.Setup(x => x.GetMathingCofinguration(It.IsAny<HttpRequest>()))
               .Returns(domainConfiguration);

            hostResolver.Setup(x => x.GetHostUri(It.IsAny<HttpRequest>(), It.IsAny<DomainConfiguration>()))
                .Returns(new Uri("http://dummy.com"));

            var hostClient = new HostClient(httpClient, hostResolver.Object, exposedConfig);
            SetRequestMethod(method);

            await hostClient.ReverseProxy(httpContext);

            Assert.AreEqual(3, httpContext.Response.Headers.Count);
            
            Assert.AreEqual("1", httpContext.Response.Headers["CustomHeader"].ToString());
            Assert.AreEqual("2", httpContext.Response.Headers["CustomHeader2"].ToString());
            
            httpContext.Response.Body.Flush();
            httpContext.Response.Body.Position = 0;
            
            using (var bodyStream = new StreamReader(httpContext.Response.Body))
            {
                
                Assert.AreEqual(responseBodyString, bodyStream.ReadToEnd());
            }
        }
    }
}
