using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace api
{
    public interface IUserLocator
    {
        string Find(HttpRequest request);
    } 

    public class BearerHeaderUserLocator : IUserLocator
    {
        public string Find(HttpRequest request)
        {
            string auth = request.Headers.First(h => h.Key.ToLower() == "authorization").Value;
            SimpleToken claims;
            if (SimpleToken.TryParse(auth.ToLower().Replace("bearer", "").Trim(), out claims))
            {
                return claims.FirstOrDefault(c => c.Key == "user")?.Value;
            }

            return null;
        }
    }
}