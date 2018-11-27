using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MySql.Data.MySqlClient;
using api;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private const string _dbConString = "server=mysql;port=3306;database=sut;user=root;password=root";
        private readonly ICommentReader _reader;
        private readonly ICommentsReader _allReader;

        public CommentsController(ICommentReader reader, ICommentsReader allReader)
        {
            _reader = reader;
            _allReader = allReader;
        }

        // GET api/comments/{id}
        [HttpGet("{id}", Name="GetComment")]
        public async Task<ActionResult<Comment>> Get(int id)
        {
            return await _reader.Get(id);
        }

        [HttpGet]
        public async Task<ActionResult<Comment[]>> GetAll()
        {
            var comments = await _allReader.GetAll();
            return comments.ToArray();
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
    }
}
