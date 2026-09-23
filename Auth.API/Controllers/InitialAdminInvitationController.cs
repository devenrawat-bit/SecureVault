using Auth.Application.DTOs.Invitations;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/internal/invitations")]
[Authorize(Roles = "Super Admin")]
public class InitialAdminInvitationController : ControllerBase
{
    private readonly IInvitationService _invitationService;

    public InitialAdminInvitationController(
        IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    [HttpPost("{organizationId:guid}")]
    public async Task<IActionResult> CreateInitialAdminInvitation(
        Guid organizationId,
        CreateInitialAdminInvitationRequest request)
    {
        var result =
            await _invitationService.CreateInitialAdminInvitationAsync(
                request,
                organizationId);

        return Ok(result);
    }
}