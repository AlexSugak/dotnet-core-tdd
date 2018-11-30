using System;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Http;
using AutoFixture.Xunit2;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using MySql.Data.MySqlClient;
using FluentAssertions;
using Xunit;

using api;
using AutoFixture;

namespace test
{
    public class EndToEndTests : IClassFixture<WebApplicationFactory<api.Startup>>
    {
        private readonly WebApplicationFactory<api.Startup> _factory;
        private const string _dbConString = "server=mysql;port=3306;database=sut;user=root;password=root";

        public EndToEndTests(WebApplicationFactory<api.Startup> factory)
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

        [Fact]
        public async Task api_works()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api");
            response.EnsureSuccessStatusCode();
        }

        [Theory, AutoData]
        [UseDatabase(_dbConString)]
        public async Task getting_comment_returns_posted_comment(
            string topic,
            string body,
            string user)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", new [] {$"Bearer user={user}"});
            await client.PostAsync(
                $"/api/topics/{topic}/comments", 
                Json("{'body': '" + body + "'}"));

            var response = await client.GetAsync($"/api/topics/{topic}/comments/1");
            
            response.StatusCode.Should().Be(HttpStatusCode.OK, "we expect api to return OK");
            var actual = await response.Content.ReadAsStringAsync();
            var expected = ("{'id':1,'topic':'" + topic + "','body':'" + body + "','user':'" + user + "'}")
                .Replace("'", "\"");
            Assert.Equal(expected, actual);
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

        private StringContent Json(string s) => new StringContent(s.Replace("'", "\""), Encoding.UTF8, "application/json");

        [Theory, AutoData]
        [UseDatabase(_dbConString)]
        public async Task posting_comment_writes_to_db(
            string topic,
            string body, 
            string user)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", new [] {$"Bearer user={user}"});
            var response = await client.PostAsync(
                $"/api/topics/{topic}/comments", 
                Json("{'body': '" + body + "'}"));
            
            response.StatusCode.Should().Be(HttpStatusCode.Created, "we expect api to create new comment");

            await ExecOnDb(async con =>
            {
                var comments = await con.QueryAsync($"select body, topic from comments where user = '{user}'");
                Assert.NotEmpty(comments);
                ((string)comments.First().body).Should().Be(body, "we expect api to save our comment");
                ((string)comments.First().topic).Should().Be(topic, "we expect api to save our topic");
            });
        }

        [Theory, AutoData]
        [UseDatabase(_dbConString)]
        public async Task getting_comments_reads_from_db(
            string topic,
            string topic2,
            string body1, 
            string body2, 
            string body3, 
            string user)
        {
            await ExecOnDb(async con =>
            {
                var count = await con.ExecuteAsync(
                    $"insert into comments (body, topic, user) values (@body1, @topic, @user), (@body2, @topic, @user), (@body3, @topic2, @user);",
                    new { body1, body2, body3, user, topic, topic2 });
            });

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/topics/{topic}/comments");
            
            response.StatusCode.Should().Be(HttpStatusCode.OK, "we expect api to return OK");
            var actual = await response.Content.ReadAsStringAsync();
            var expected = ("[{'id':1,'topic':'" + topic + "','body':'" + body1 + "','user':'" + user + "'},{'id':2,'topic':'" + topic + "','body':'" + body2 + "','user':'" + user + "'}]")
                .Replace("'", "\"");
            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        [UseDatabase(_dbConString)]
        public async Task getting_single_comment_reads_from_db(
            string topic,
            string body1, 
            string body2, 
            string user)
        {
            await ExecOnDb(async con =>
            {
                var count = await con.ExecuteAsync(
                    $"insert into comments (body, topic, user) values (@body1, @topic, @user), (@body2, @topic, @user);",
                    new { body1, body2, user, topic });
            });

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/topics/{topic}/comments/2");
            
            response.StatusCode.Should().Be(HttpStatusCode.OK, "we expect api to return OK");
            var actual = await response.Content.ReadAsStringAsync();
            var expected = ("{'id':2,'topic':'" + topic + "','body':'" + body2 + "','user':'" + user + "'}")
                .Replace("'", "\"");
            Assert.Equal(expected, actual);
        }

        [Theory, AutoData]
        [UseDatabase(_dbConString)]
        public async Task getting_unknown_comment_id_returns_404(
            string topic,
            string comment,
            string user,
            Generator<int> generator)
        {
            await ExecOnDb(async con =>
            {
                var count = await con.ExecuteAsync(
                    $"insert into comments (body, topic, user) values (@body, @topic, @user);",
                    new { body = comment, user, topic });
            });
            var unknownId = generator.Where(i => i > 1).Take(1).First();

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/topics/{topic}/comments/{unknownId}");
            
            response.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect api to return 404");
        }

        [Theory, AutoData]
        [UseDatabase(_dbConString)]
        public async Task getting_unknown_topic_returns_404(
            string topic,
            string unknownTopic,
            string body,
            string user)
        {
            await ExecOnDb(async con =>
            {
                var count = await con.ExecuteAsync(
                    $"insert into comments (body, topic, user) values (@body, @topic, @user);",
                    new { body, user, topic });
            });

            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/topics/{unknownTopic}/comments/1");
            
            response.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect api to return 404");
        }
    }
}
