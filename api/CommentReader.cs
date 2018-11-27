using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace api
{
    public class Comment 
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public string User { get; set; }
    }

    public interface ICommentReader
    {
        Task<Comment> Get(int id);
    }

    public interface ICommentsReader
    {
        Task<IEnumerable<Comment>> GetAll();
    }

    public class CommentDbReader : ICommentReader, ICommentsReader
    {
        private readonly string _dbConString;

        public CommentDbReader(string dbConString)
        {
            _dbConString = dbConString;
        }

        public async Task<Comment> Get(int id)
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                var comment = await con.QueryFirstAsync<Comment>("select * from comments where id = @id", new { id });

                return comment;
            }
        }

        public async Task<IEnumerable<Comment>> GetAll()
        {
            using(var con = new MySqlConnection(_dbConString))
            {
                con.Open();
                return await con.QueryAsync<Comment>("select * from comments");
            }
        }
    } 
}