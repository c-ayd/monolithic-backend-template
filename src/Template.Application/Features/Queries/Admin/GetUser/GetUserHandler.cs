using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.Mediator.Abstractions;
using Template.Application.Abstractions.UOW;

namespace Template.Application.Features.Queries.Admin.GetUser
{
    public class GetUserHandler : IAsyncHandler<GetUserRequest, ExecResult<GetUserResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ExecResult<GetUserResponse>> HandleAsync(GetUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetWithFullContextByIdAsync(request.Id!.Value, cancellationToken);
            if (user == null)
                return new ExecNotFound();

            return new GetUserResponse()
            {
                User = user
            };
        }
    }
}
