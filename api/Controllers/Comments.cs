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
        private readonly ICommentWriter _writer;
        private readonly IUserLocator _userLocator;

        public CommentsController(
            ICommentReader reader, 
            ICommentWriter writer,
            IUserLocator userLocator)
        {
            _reader = reader;
            _writer = writer;
            _userLocator = userLocator;
        }

        // GET api/comments/{id}
        [HttpGet("{id}", Name="GetComment")]
        public async Task<ActionResult<Comment>> Get(int id)
        {
            var comment = await _reader.Read(id);
            if (comment == null) 
            {
                return NotFound();
            } 

            return comment;
        }

        [HttpGet]
        public async Task<ActionResult<Comment[]>> GetAll()
        {
            var comments = await _reader.ReadAll();
            return comments.ToArray();
        }

        // POST api/comments
        [HttpPost]
        public async Task<IActionResult> Create(Comment comment)
        {
            comment.User = _userLocator.Find(this.Request);
            int id;
            try
            {
                id = await _writer.Write(comment);
            }
            catch(ValidationException e)
            {
                return BadRequest(e.Message);
            }

            return CreatedAtRoute("GetComment", new { id = id }, comment);
        }
    }
}
