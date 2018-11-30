using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api;
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Xunit;

namespace test
{
    public class CommentValidatorTests
    {
        private readonly Fixture _fixture;

        public CommentValidatorTests()
        {
            _fixture = new Fixture();
        }

        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData("")]
        public void InfoValidate_returns_errors_on_missing_user(
            string invalidUser,
            Comment comment)
        {
            comment.User = invalidUser;
            var sut = new CommentInfoValidator();

            var result = sut.Validate(comment);

            result.Should().NotBeEmpty();
        }

        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData("")]
        public void InfoValidate_returns_errors_on_empty_comment(
            string invalidComment,
            Comment comment)
        {
            comment.Body = invalidComment;
            var sut = new CommentInfoValidator();

            var result = sut.Validate(comment);

            result.Should().NotBeEmpty();
        }

        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData("")]
        public void InfoValidate_returns_errors_on_empty_topic(
            string invalidTopic,
            Comment comment)
        {
            comment.Topic = invalidTopic;
            var sut = new CommentInfoValidator();

            var result = sut.Validate(comment);

            result.Should().NotBeEmpty();
        }

        [Theory, AutoData]
        public void InfoValidate_returns_errors_on_long_comment(
            Generator<int> generator)
        {
            var commentLength = generator.Where(i => i > 140).Take(1).First();
            var comment = _fixture.Build<Comment>()
                            .With(x => x.Body,
                                  new SpecimenContext(new RandomStringOfLengthGenerator())
                                        .Resolve(new RandomStringOfLengthRequest(commentLength)))
                            .Create();
            var sut = new CommentInfoValidator();

            var result = sut.Validate(comment);

            result.Should().NotBeEmpty();
        }

        [Theory, AutoData]
        public void InfoValidate_returns_errors_on_long_topic(
            Generator<int> generator)
        {
            var topicLength = generator.Where(i => i > 50).Take(1).First();
            var comment = _fixture.Build<Comment>()
                            .With(x => x.Topic,
                                  new SpecimenContext(new RandomStringOfLengthGenerator())
                                        .Resolve(new RandomStringOfLengthRequest(topicLength)))
                            .Create();
            var sut = new CommentInfoValidator();

            var result = sut.Validate(comment);

            result.Should().NotBeEmpty();
        }

        [Theory, AutoData]
        public void InfoValidate_returns_no_errors_on_valid_user(
            Comment comment)
        {
            var sut = new CommentInfoValidator();

            var result = sut.Validate(comment);

            result.Should().BeEmpty();
        }
    }
}