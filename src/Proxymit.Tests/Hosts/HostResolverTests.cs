using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proxymit.Tests.Hosts
{
    [TestFixture]
    public class HostResolverTests
    {
        private Mock<HttpRequest> httpRequest;

        [SetUp]
        public void Init()
        {
            httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup(x => x.Host)
                .Returns(() => new HostString("test.com"));

            httpRequest.Setup(x => x.Path)
               .Returns(() => new PathString(""));
        }
    }
}
