using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.Success;
using MediatR;
using Template.Application.Abstractions.UOW;

namespace Template.Application.Features.Queries.Admin.GetUser
{
    public class GetUserHandler : IRequestHandler<GetUserRequest, ExecResult<GetUserResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ExecResult<GetUserResponse>> Handle(GetUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetWithFullContextByIdAsync(request.Id!.Value, cancellationToken);
            if (user == null)
                return new ExecNoContent<GetUserResponse>();

            return new GetUserResponse()
            {
                User = user
            };
        }
    }
}
