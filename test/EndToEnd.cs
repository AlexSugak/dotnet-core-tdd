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
using AutoFixture.Xunit2;

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

        private async Task ExecOnDb(Func<MySqlConnection, Task> func)
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                await func(con);
            }
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
            await ExecOnDb(async con =>
            {
                var version = await con.QueryAsync<string>("select version()");
                Assert.NotEmpty(version);
            });
        }

        // now let's do the spike

        private StringContent Json(string s) => new StringContent(s.Replace("'", "\""), Encoding.UTF8, "application/json");

        [Theory, AutoData]
        [UseDatabase(_dbConString)]
        public async Task posting_comment_writes_to_db(string comment, string user)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", new [] {$"Bearer user={user}"});
            var response = await client.PostAsync(
                "/api/comments", 
                Json("{'body': '" + comment + "'}"));
            
            response.StatusCode.Should().Be(HttpStatusCode.Created, "we expect api to create new comment");

            await ExecOnDb(async con =>
            {
                var comments = await con.QueryAsync<string>($"select body from comments where user = '{user}'");
                Assert.NotEmpty(comments);
                comments.First().Should().Be(comment, "we expect api to save our comment");
            });
        }

        [Theory, AutoData]
        [UseDatabase(_dbConString)]
        public async Task getting_comments_reads_from_db(string comment1, string comment2, string user)
        {
            await ExecOnDb(async con =>
            {
                var count = await con.ExecuteAsync(
                    $"insert into comments (body, user) values (@body1, @user), (@body2, @user);",
                    new { body1 = comment1, body2 = comment2, user = user });
            });

            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/comments");
            
            response.StatusCode.Should().Be(HttpStatusCode.OK, "we expect api to return OK");
            var actual = await response.Content.ReadAsStringAsync();
            var expected = ("[{'id':1,'body':'" + comment1 + "','user':'" + user + "'},{'id':2,'body':'" + comment2 + "','user':'" + user + "'}]")
                .Replace("'", "\"");
            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        [UseDatabase(_dbConString)]
        public async Task getting_single_comment_reads_from_db(string comment1, string comment2, string user)
        {
            await ExecOnDb(async con =>
            {
                var count = await con.ExecuteAsync(
                    $"insert into comments (body, user) values (@body1, @user), (@body2, @user);",
                    new { body1 = comment1, body2 = comment2, user = user });
            });

            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/comments/2");
            
            response.StatusCode.Should().Be(HttpStatusCode.OK, "we expect api to return OK");
            var actual = await response.Content.ReadAsStringAsync();
            var expected = ("{'id':2,'body':'" + comment2 + "','user':'" + user + "'}")
                .Replace("'", "\"");
            Assert.Equal(expected, actual);
        }
    }
}
