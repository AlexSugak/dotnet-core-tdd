using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace api
{
    public class CommentDbStore : ICommentReader, ICommentWriter
    {
        private readonly string _dbConString;

        public CommentDbStore(string dbConString)
        {
            _dbConString = dbConString;
        }

        public async Task<Comment> Read(int id)
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                var comment = await con.QueryAsync<Comment>("select * from comments where id = @id", new { id });

                return comment.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Comment>> ReadAll()
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                return await con.QueryAsync<Comment>("select * from comments");
            }
        }

        public async Task<int> Write(Comment comment)
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();

                var cmd = @"
                    insert into comments (body, user) values (@body, @user);
                    select LAST_INSERT_ID();";
                return (await con.QueryAsync<int>(cmd, new { body = comment.Body, user = comment.User })).Single();
            }
        }
    } 
}