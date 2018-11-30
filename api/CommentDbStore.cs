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

        public async Task<Comment> Read(string topic, int id)
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                var comment = await con.QueryAsync<Comment>(
                    "select * from comments where id = @id and topic = @topic ", 
                    new { id, topic });

                return comment.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Comment>> ReadAll(string topic)
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                return await con.QueryAsync<Comment>("select * from comments where topic = @topic", new { topic });
            }
        }

        public async Task<int> Write(Comment comment)
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();

                var cmd = @"
                    insert into comments (body, topic, user) values (@body, @topic, @user);
                    select LAST_INSERT_ID();";
                return (await con.QueryAsync<int>(cmd, new { topic = comment.Topic, body = comment.Body, user = comment.User })).Single();
            }
        }
    } 
}