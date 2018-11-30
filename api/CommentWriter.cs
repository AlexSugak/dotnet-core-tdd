using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace api
{
    public interface ICommentWriter
    {
        Task<int> Write(Comment comment);
    }
}