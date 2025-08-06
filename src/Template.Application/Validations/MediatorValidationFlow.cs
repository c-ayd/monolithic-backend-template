using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.Mediator.Abstractions;
using Cayd.AspNetCore.Mediator.Flows;
using FluentValidation;

namespace Template.Application.Validations
{
    public class MediatorValidationFlow<TRequest, TResponse> : IMediatorFlow<TRequest, TResponse>
        where TRequest : IAsyncRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public MediatorValidationFlow(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> InvokeAsync(TRequest request, AsyncHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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

            return await next();
        }
    }
}
