using System;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using MySql.Data.MySqlClient;
using Xunit;

using api;

namespace test
{
    public class EndToEnd : IClassFixture<WebApplicationFactory<api.Startup>>
    {
        private readonly WebApplicationFactory<api.Startup> _factory;
        private readonly string _dbConString = "server=mysql;port=3306;database=sut;user=root;password=root";

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

        private StringContent Json(string s) => new StringContent(s.Replace("'", "\""));

        [Fact]
        public async Task posting_comment_writes_to_db()
        {
            var client = _factory.CreateClient();
            var comment = "hello from integration test";
            var response = await client.PostAsync(
                "/api/comments", 
                Json("{'body': '" + comment + "'}"));
            response.EnsureSuccessStatusCode();

            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                var version = await con.QueryAsync<string>($"select * from comments where body = '{comment}'");
                Assert.NotEmpty(version);
            }
        }
    }
}
