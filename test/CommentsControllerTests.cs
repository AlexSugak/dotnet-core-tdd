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

namespace test
{
    public class CommentsControllerTests
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_must_call_comment_reader(
            int id, 
            Comment comment,
            Mock<ICommentReader> reader,
            Mock<ICommentsReader> allreader)
        {
            reader.Setup(r => r.Get(id)).Returns(Task.FromResult(comment));
            var sut = new CommentsController(reader.Object, allreader.Object);

            var result = await sut.Get(id);

            reader.Verify(r => r.Get(id), Times.Exactly(1));
            result.Value.Should().Be(comment);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetAll_must_call_comments_reader(
            int id, 
            Comment comment1,
            Comment comment2,
            Mock<ICommentReader> creader,
            Mock<ICommentsReader> csreader)
        {
            IEnumerable<Comment> comments()
            {
                yield return comment1;
                yield return comment2;
            } 
            csreader.Setup(r => r.GetAll()).Returns(Task.FromResult(comments()));
            var sut = new CommentsController(creader.Object, csreader.Object);
            
            var result = await sut.GetAll();

            csreader.Verify(r => r.GetAll(), Times.Exactly(1));
            result.Value.Should().BeEquivalentTo(comments());
        }
    }
}