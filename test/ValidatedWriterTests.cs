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
    public class ValidatedWriterTests
    {
        [Theory, AutoMoqData]
        public async Task Write_calls_inner_writer(
            Comment comment,
            Mock<ICommentValidator> validator,
            Mock<ICommentWriter> writer) 
        {
            var sut = new ValidatedWriter(validator.Object, writer.Object);

            await sut.Write(comment);

            writer.Verify(w => w.Write(comment), Times.Exactly(1));
        }

        [Theory, AutoMoqData]
        public async Task Write_throws_on_invalid_comment(
            Comment comment,
            List<ValidationError> errors,
            Mock<ICommentValidator> validator,
            Mock<ICommentWriter> writer) 
        {
            validator.Setup(v => v.Validate(comment)).Returns(errors);
            var sut = new ValidatedWriter(validator.Object, writer.Object);

            await Assert.ThrowsAsync<ValidationException>(() => sut.Write(comment));
        }
    }
}