using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Xunit;

using api;

namespace test
{
    public class SimpleTokenTests
    {
        [Fact]
        public void token_is_a_collection_of_claims()
        {
            var tkn = new SimpleToken();
            Assert.IsAssignableFrom<IEnumerable<Claim>>(tkn);
        }
        
        [Theory]
        [AutoData]
        public void token_returns_passed_claims(List<Claim> claims)
        {
            var tkn = new SimpleToken(claims);
            tkn.Should().BeEquivalentTo(claims);
        }

        [Theory]
        [InlineData("", new string[] { })]
        [InlineData("user=bob", new [] { "user", "bob" })]
        [InlineData("user=bob&role=admin", new [] { "user", "bob", "role", "admin" })]
        public void parse_returns_correct_claims_on_valid_string(string token, string[] claims)
        {
            var expected = claims
                .Where((x, i) => i % 2 == 0)
                .Zip(claims.Where((x, i) => i % 2 != 0), Tuple.Create)
                .Select(pair => new Claim(pair.Item1, pair.Item2));
            SimpleToken tkn;
            var parsed = SimpleToken.TryParse(token, out tkn);

            parsed.Should().Be(true, "because we passed correct token string");
            tkn.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("foo")]
        [InlineData("foo=bar&baz")]
        public void parse_returns_false_on_invalid_string(string token)
        {
            SimpleToken tkn;
            var parsed = SimpleToken.TryParse(token, out tkn);

            parsed.Should().Be(false, "because we passed incorrect token string");
            tkn.Should().BeNull();
        }
    }
}