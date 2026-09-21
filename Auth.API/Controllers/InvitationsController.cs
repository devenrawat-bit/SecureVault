using Auth.Application.DTOs.Invitations;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Organization Admin")]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationService _invitationService;
        public InvitationsController(IInvitationService invitationService)
        {
            _invitationService = invitationService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateInvitation(
        CreateInvitationRequest request)
        {
            // Temporary organization ID.
            // We'll replace this with the Organization Admin's JWT claim
            // once JWT authentication is wired up.
            var organizationId = Guid.Empty;

            var result = await _invitationService.CreateInvitationAsync(
                request,
                organizationId);

            return Ok(result);
        }
    }
}
