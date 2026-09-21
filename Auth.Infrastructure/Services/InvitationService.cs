using Auth.Application.DTOs.Invitations;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence;
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

    public async Task<InvitationCreatedResponse> CreateInvitationAsync(
        CreateInvitationRequest request,
        Guid organizationId)
    {
        // Business logic will go here.
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
            TokenHash = tokenHash,
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