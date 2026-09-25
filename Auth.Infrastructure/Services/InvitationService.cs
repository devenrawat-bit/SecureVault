using Auth.Application.DTOs.Invitations;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Infrastructure.Services;

public class InvitationService : IInvitationService
{
    private readonly AppDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public InvitationService(AppDbContext context, RoleManager<IdentityRole<Guid>> roleManager) 
    {
        _roleManager = roleManager;
        _context = context;
    }

    /// <summary>
    /// This method creates an initial admin invitation for a given organization. once the org is created by the super admin
    /// </summary>
    /// <param name="request"></param>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<InvitationCreatedResponse> CreateInitialAdminInvitationAsync(CreateInitialAdminInvitationRequest request, Guid organizationId)
    {
        //the caller can not choose the role 
        const string role = "Organization Admin";

        var roleExists = await _roleManager.RoleExistsAsync(role);

        if (!roleExists)
        {
            throw new InvalidOperationException(
                $"Role '{role}' does not exist.");
        }

        var tokenBytes = RandomNumberGenerator.GetBytes(32);

        var rawToken = Convert.ToBase64String(tokenBytes);

        var tokenHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(rawToken)));

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = role,
            TokenHash = tokenHash, //storing the hash in the db, and not the real token itself 
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Invitations.AddAsync(invitation);

        await _context.SaveChangesAsync();

        return new InvitationCreatedResponse
        {
            InvitationId = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role,
            InvitationToken = rawToken,
            ExpiresAt = invitation.ExpiresAt
        };
    }

    /// <summary>
    /// The above invitation is for the admin that freshely starts the organization, but this one is for the workers that are working in the organzation apart from the admin role like developers, qa, team lead etc...
    /// </summary>
    /// <param name="request"></param>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<InvitationCreatedResponse> CreateInvitationAsync(
        CreateInvitationRequest request,
        Guid organizationId)
    {
        
        var roleExists = await _roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
        {
            throw new InvalidOperationException(
                $"Role '{request.Role}' does not exist.");
        }

        var tokenBytes = RandomNumberGenerator.GetBytes(32);

        var rawToken = Convert.ToBase64String(tokenBytes);
        var tokenHash = Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            TokenHash = tokenHash, //hash token instead of the raw token in the db for the security reasons.
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Invitations.AddAsync(invitation);

        await _context.SaveChangesAsync();

        return new InvitationCreatedResponse
        {
            InvitationId = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role,
            InvitationToken = rawToken,
            ExpiresAt = invitation.ExpiresAt
        };
    }
}