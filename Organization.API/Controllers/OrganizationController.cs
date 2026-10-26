using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Organization.Application.DTOs.Organizations;
using Organization.Application.Interfaces;

namespace Organization.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Super Admin")]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(
        IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    /// <summary>
    /// This API endpoint is used to create an organization 
    /// </summary>
    /// <param name="request">The data for the creation of the organization</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateOrganization(
        CreateOrganizationRequest request)
    {
        var organization =
            await _organizationService.CreateOrganizationAsync(request);

        return Ok(organization);
    }
}