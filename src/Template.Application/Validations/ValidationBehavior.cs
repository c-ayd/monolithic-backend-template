using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using FluentValidation;
using MediatR;

namespace Template.Application.Validations
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                foreach (var validator in _validators)
                {
                    if (validator == null)
                        continue;

                    var validationResult = await validator.ValidateAsync(request, cancellationToken);
                    if (validationResult.Errors.Count > 0)
                    {
                        var errorDetails = validationResult.Errors
                            .Select(e => new ExecErrorDetail(e.ErrorMessage, e.ErrorCode))
                            .ToList();

                        return (dynamic)new ExecBadRequest(errorDetails);
                    }
                }
            }

            return await next();
        }
    }
}
