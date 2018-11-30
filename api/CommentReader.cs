using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task<Comment> Read(int id);
        Task<IEnumerable<Comment>> ReadAll();
    }
}