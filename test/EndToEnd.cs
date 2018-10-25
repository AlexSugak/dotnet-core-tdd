using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Xunit;

using api;

namespace test
{
    public class EndToEnd : IClassFixture<WebApplicationFactory<api.Startup>>
    {
        private readonly WebApplicationFactory<api.Startup> _factory;

        public EndToEnd(WebApplicationFactory<api.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task api_root_returns_success()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api");
            response.EnsureSuccessStatusCode();
        }
    }
}
