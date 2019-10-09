using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Proxymit.Core.Configs;
using Proxymit.Core.Hosts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proxymit.Tests.Hosts
{
    [TestFixture]
    public class HostResolverTests
    {
        private Mock<IDomainConfigurationLoader> domainLoader;
        private List<DomainConfiguration> domainConfigurations;


        [SetUp]
        public void Init()
        {
            domainConfigurations = new List<DomainConfiguration> {
                new DomainConfiguration{
                    ChallengeType = "",
                    Destination = "destination1",
                    Domain = "test.com",
                    Path = ""
                },

                new DomainConfiguration{
                    ChallengeType = "",
                    Destination = "destination3",
                    Domain = "test.com",
                    Path = "/api"
                },
                new DomainConfiguration
                {
                    ChallengeType = "",
                    Destination = "destination2",
                    Domain = "test2.com",
                    Path = ""
                }
            };
            domainLoader = new Mock<IDomainConfigurationLoader>();


            domainLoader.Setup(x => x.GetConfigurations())
                .Returns(domainConfigurations.AsQueryable());

        }

        private Mock<HttpRequest> SetupHttpRequest(string domain, string path, bool isHttps)
        {
            Mock<HttpRequest> httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup(x => x.Host)
                .Returns(() => new HostString(domain));

            httpRequest.Setup(x => x.Path)
               .Returns(() => new PathString(path));

            httpRequest.Setup(x => x.IsHttps)
                .Returns(isHttps);

            return httpRequest;
        }

        [Test]
        [TestCase("test.com", "", false, "http://destination1/")]
        [TestCase("test.com", "/uma/url/customizada", false, "http://destination1/uma/url/customizada")]
        [TestCase("test2.com", "", true, "https://destination2/")]
        [TestCase("test.com", "/api", true, "https://destination3/api")]
        public void Should_MatchDomains(string domain, string path, bool isHttps, string expectedResult)
        {
            HostResolver hostResolver = new HostResolver(domainLoader.Object);
            var httpRequest = this.SetupHttpRequest(domain, path, isHttps);
            var uri  = hostResolver.HostUri(httpRequest.Object);

            Assert.AreEqual(expectedResult, uri.ToString());
        }
    }
}
