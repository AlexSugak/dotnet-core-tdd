using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Xunit;

namespace test
{
    public class CommentValidatorTests
    {
        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData("")]
        public void InfoValidate_returns_errors_on_missing_user(
            string invalidUser,
            Comment comment)
        {
            comment.User = invalidUser;
            var sut = new CommentInfoValidator();

            IEnumerable<ValidationError> result = sut.Validate(comment);

            result.Should().NotBeEmpty();
        }

        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData("")]
        public void InfoValidate_returns_errors_on_wrong_comment(
            string invalidComment,
            Comment comment)
        {
            comment.Body = invalidComment;
            var sut = new CommentInfoValidator();

            IEnumerable<ValidationError> result = sut.Validate(comment);

            result.Should().NotBeEmpty();
        }

        [Theory]
        [AutoData]
        public void InfoValidate_returns_no_errors_on_valid_user(
            Comment comment)
        {
            var sut = new CommentInfoValidator();

            IEnumerable<ValidationError> result = sut.Validate(comment);

            result.Should().BeEmpty();
        }
    }
}