using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Api.Policies;
using Template.Api.Utilities;
using Template.Application.Features.Queries.Admin.GetUser;
using Template.Application.Features.Queries.Admin.GetUsers;
using Template.Application.Mappings.Controllers.Admin;

namespace Template.Api.Controllers
{
    [Authorize(Roles = AdminPolicy.RoleName)]
    [ApiController]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly ISender _sender;

        public AdminController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetUsersRequest()
            {
                Page = page,
                PageSize = pageSize
            }, cancellationToken);

            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, AdminMappings.Map(response), metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUser(Guid? id, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new GetUserRequest()
            {
                Id = id
            }, cancellationToken);

            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, AdminMappings.Map(response), metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }
    }
}
