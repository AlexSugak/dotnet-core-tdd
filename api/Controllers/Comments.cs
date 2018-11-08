using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MySql.Data.MySqlClient;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private const string _dbConString = "server=mysql;port=3306;database=sut;user=root;password=root";

        public class Comment 
        {
            public int Id { get; set; }
            public string Body { get; set; }
            public string User { get; set; }
        }

        // GET api/comments/{id}
        [HttpGet("{id}", Name="GetComment")]
        public async Task<ActionResult<Comment>> Get(int id)
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                var comment = await con.QueryFirstAsync<Comment>("select * from comments where id = @id", new { id });

                return comment;
            }
        }

        // POST api/comments
        [HttpPost]
        public async Task<IActionResult> Create(Comment comment)
        {
            int id;
            string auth = this.Request.Headers.First(h => h.Key.ToLower() == "authorization").Value;
            // user=<name>
            string user = auth.ToLower().Replace("bearer", "").Trim().Split("=").Skip(1).Take(1).First();
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();

                var cmd = @"
                    insert into comments (body, user) values (@body, @user);
                    select LAST_INSERT_ID();";
                id = (await con.QueryAsync<int>(cmd, new { body = comment.Body, user = user })).Single();
            }

            return CreatedAtRoute("GetComment", new { id = id }, comment);
        }

        [HttpGet]
        public async Task<ActionResult<Comment[]>> GetAll()
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                var comments = await con.QueryAsync<Comment>("select * from comments");

                return comments.ToArray();
            }
        }
    }
}
