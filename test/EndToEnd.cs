using System;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using MySql.Data.MySqlClient;
using FluentAssertions;
using Xunit;

using api;
using System.Net;
using System.Text;

namespace test
{
    public class EndToEnd : IClassFixture<WebApplicationFactory<api.Startup>>
    {
        private readonly WebApplicationFactory<api.Startup> _factory;
        private const string _dbConString = "server=mysql;port=3306;database=sut;user=root;password=root";

        public EndToEnd(WebApplicationFactory<api.Startup> factory)
        {
            _factory = factory;
        }

        // break the ice

        [Fact]
        public async Task api_works()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task db_works()
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                var version = await con.QueryAsync<string>("select version()");
                Assert.NotEmpty(version);
            }
        }

        // now let's do the spike

        private StringContent Json(string s) => new StringContent(s.Replace("'", "\""), Encoding.UTF8, "application/json");

        [Fact]
        [UseDatabase(_dbConString)]
        public async Task posting_comment_writes_to_db()
        {
            var client = _factory.CreateClient();
            var comment = "hello from integration test";
            var response = await client.PostAsync(
                "/api/comments", 
                Json("{'body': '" + comment + "'}"));
            
            response.StatusCode.Should().Be(HttpStatusCode.Created, "we expect api to create new comment");

            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                var version = await con.QueryAsync<string>($"select * from comments where body = '{comment}'");
                Assert.NotEmpty(version);
            }
        }
    }
}
