using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace api
{
    public class SimpleToken : IEnumerable<Claim>
    {
        private readonly IEnumerable<Claim> _claims;

        public SimpleToken(params Claim[] claims)
        {
            _claims = claims;
        }

        public SimpleToken(IEnumerable<Claim> claims)
        {
            _claims = claims;
        }

        public static bool TryParse(string input, out SimpleToken claims)
        {
            if (input == null) 
            {
                claims = null;
                return false;
            }

            var claimTokens = input.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if(claimTokens.Any(t => !t.Contains("=")) 
            || claimTokens.Any(string.IsNullOrEmpty))
            {
                claims = null;
                return false;
            }

            claims = new SimpleToken(claimTokens
                                        .Select(t => 
                                        { 
                                            var tt = t.Split('=');
                                            return new Claim(tt[0], tt[1]);
                                        })
                                        .ToArray());

            return true;
        }

        public IEnumerator<Claim> GetEnumerator()
        {
            return _claims.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Claim
    {   
        public string Key {get; private set;}
        public string Value {get; private set;}

        public Claim(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}