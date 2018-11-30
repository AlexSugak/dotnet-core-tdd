using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace api
{
    [System.Serializable]
    public class ValidationException : System.Exception
    {
        private ValidationError[] errors;

        public ValidationException() { }
        public ValidationException(string message) : base(message) { }

        public ValidationException(IEnumerable<ValidationError> errors)
            : this("Validation errors occured: " + String.Join(", ", errors.Select(e => e.Error))){}
        public ValidationException(string message, System.Exception inner) : base(message, inner) { }
        protected ValidationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class ValidationError
    {
        public string Error { get; set; }
    }

    public interface ICommentValidator
    {
        IEnumerable<ValidationError> Validate(Comment comment);
    }

	public class CommentInfoFluentValidator: AbstractValidator<Comment> 
	{
		public CommentInfoFluentValidator()
		{
			RuleFor(x => x.User).NotEmpty();
			RuleFor(x => x.Body).NotEmpty().MaximumLength(140);
		}
	}

    public class CommentInfoValidator : ICommentValidator
    {
        private readonly CommentInfoFluentValidator _validator;

        public CommentInfoValidator()
        {
            _validator = new CommentInfoFluentValidator();
        }

        public IEnumerable<ValidationError> Validate(Comment comment)
        {
            return _validator.Validate(comment)
                    .Errors
                    .Select(e => new ValidationError(){ Error = e.ErrorMessage });
        }
    }

    public class ValidatedWriter : ICommentWriter
    {
        private readonly ICommentValidator _validator;
        private readonly ICommentWriter _writer;

        public ValidatedWriter(ICommentValidator validator, ICommentWriter writer)
        {
            _validator = validator;
            _writer = writer;    
        }

        public Task<int> Write(Comment comment)
        {
            var errors = _validator.Validate(comment).ToArray();
            if (errors.Any())
            {
                throw new ValidationException(errors);
            }

            return _writer.Write(comment);
        }
    }
}