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
            public string Body { get; set; }
        }

        // GET api/comments/{id}
        [HttpGet("{id}", Name="GetComment")]
        public ActionResult<Comment> Get(int id)
        {
            return new Comment();
        }

        // POST api/comments
        [HttpPost]
        public IActionResult Create(Comment comment)
        {
            int id;
            string bearer = this.Request.Headers.First(h => h.Key.ToLower() == "bearer").Value;
            // user=<name>
            string user = bearer.Split("=").Skip(1).Take(1).First();
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();

                var cmd = @"
                    insert into comments (body, user) values (@body, @user);
                    select LAST_INSERT_ID();";
                id = con.Query<int>(cmd, new { body = comment.Body, user = user }).Single();
            }

            return CreatedAtRoute("GetComment", new { id = id }, comment);
        }
    }
}
