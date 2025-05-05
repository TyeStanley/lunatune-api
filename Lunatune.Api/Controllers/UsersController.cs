using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;

namespace Lunatune.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("me")]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        try
        {
            var auth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(auth0Id))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByAuth0IdAsync(auth0Id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while fetching the user: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var auth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(auth0Id))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required");
            }

            if (await _userService.UserExistsAsync(auth0Id))
            {
                return Conflict("User already exists");
            }

            var user = new User
            {
                Auth0Id = auth0Id,
                Email = request.Email,
                Name = request.Name,
                Picture = request.Picture,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetCurrentUser), new { id = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while creating the user: {ex.Message}");
        }
    }
}

public class CreateUserRequest
{
    public required string Email { get; set; }
    public string? Name { get; set; }
    public string? Picture { get; set; }
}
