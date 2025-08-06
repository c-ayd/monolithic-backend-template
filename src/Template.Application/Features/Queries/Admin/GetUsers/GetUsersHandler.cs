using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.Success;
using Cayd.AspNetCore.Mediator.Abstractions;
using Template.Application.Abstractions.UOW;
using Template.Application.Validations.Constants;

namespace Template.Application.Features.Queries.Admin.GetUsers
{
    public class GetUsersHandler : IAsyncHandler<GetUsersRequest, ExecResult<GetUsersResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUsersHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ExecResult<GetUsersResponse>> HandleAsync(GetUsersRequest request, CancellationToken cancellationToken)
        {
            int page = request.Page ?? 1;
            int pageSize = request.PageSize ?? 20;

            var (users, numberOfNextPages) = await _unitOfWork.Users.GetAllAsync(page, pageSize, PaginationConstants.MaxNumberOfNextPages, cancellationToken);
            if (users.Count == 0)
                return new ExecNoContent<GetUsersResponse>();

            return new ExecOk<GetUsersResponse>(new GetUsersResponse()
            {
                Users = users
            },
            new
            {
                Page = page,
                PageSize = pageSize,
                NumberOfNextPages = numberOfNextPages
            });
        }
    }
}
