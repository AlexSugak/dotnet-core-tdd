using System;
using System.Linq;
using System.Threading.Tasks;
using api;
using api.Controllers;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;
using Moq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace test
{
    public class CommentsControllerTests
    {
        [Theory, AutoMoqData]
        public async Task Get_must_call_comment_reader(
            string topic, 
            int id, 
            Comment comment,
            Mock<ICommentReader> reader,
            Mock<ICommentWriter> writer,
            Mock<IUserLocator> locator)
        {
            reader.Setup(r => r.Read(topic, id)).Returns(Task.FromResult(comment));
            var sut = new CommentsController(reader.Object, writer.Object, locator.Object);

            var result = await sut.Get(topic, id);

            reader.Verify(r => r.Read(topic, id), Times.Exactly(1));
            result.Value.Should().Be(comment);
        }

        [Theory, AutoMoqData]
        public async Task Get_must_return_404_if_reader_read_none(
            string topic,
            int id, 
            Mock<ICommentReader> reader,
            Mock<ICommentWriter> writer,
            Mock<IUserLocator> locator)
        {
            reader.Setup(r => r.Read(topic, id)).Returns(Task.FromResult<Comment>(null));
            var sut = new CommentsController(reader.Object, writer.Object, locator.Object);

            var result = await sut.Get(topic, id);
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Theory, AutoMoqData]
        public async Task GetAll_must_call_comments_reader(
            string topic,
            Comment comment1,
            Comment comment2,
            Mock<ICommentReader> reader,
            Mock<ICommentWriter> writer,
            Mock<IUserLocator> locator)
        {
            IEnumerable<Comment> comments()
            {
                yield return comment1;
                yield return comment2;
            } 
            reader.Setup(r => r.ReadAll(topic)).Returns(Task.FromResult(comments()));
            var sut = new CommentsController(reader.Object, writer.Object, locator.Object);
            
            var result = await sut.GetAll(topic);

            reader.Verify(r => r.ReadAll(topic), Times.Exactly(1));
            result.Value.Should().BeEquivalentTo(comments());
        }

        [Theory, AutoMoqData]
        public async Task Create_must_call_comment_writer(
            string topic,
            Comment comment,
            Mock<ICommentReader> reader,
            Mock<ICommentWriter> writer,
            Mock<IUserLocator> locator)
        {
            writer.Setup(w => w.Write(comment)).Returns(Task.FromResult(1));
            var sut = new CommentsController(reader.Object, writer.Object, locator.Object);

            var result = await sut.Create(topic, comment);

            writer.Verify(r => r.Write(comment), Times.Exactly(1));
        }

        [Theory, AutoMoqData]
        public async Task Create_must_return_400_if_comment_not_valid(
            string topic,
            Comment comment,
            Mock<ICommentReader> reader,
            Mock<ICommentWriter> writer,
            Mock<IUserLocator> locator)
        {
            writer.Setup(w => w.Write(comment)).ThrowsAsync(new ValidationException());
            var sut = new CommentsController(reader.Object, writer.Object, locator.Object);

            var result = await sut.Create(topic, comment);

            Assert.IsAssignableFrom<BadRequestObjectResult>(result);
        }
    }
}