using AutoMapper;
using DietiEstate.Shared.Dtos.Filters;
using DietiEstate.Shared.Dtos.Requests;
using DietiEstate.Shared.Dtos.Responses;
using DietiEstate.Shared.Enums;
using DietiEstate.Shared.Models.UserModels;
using DietiEstate.WebApi.Extensions;
using DietiEstate.WebApi.Repositories.Interfaces;
using DietiEstate.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DietiEstate.WebApi.Controllers;

/// <summary>
/// Handles user-related operations such as CRUD operations and user queries.
/// </summary>
/// <remarks>
/// This controller manages operations for user data, including fetching user details,
/// creating new users, updating existing user information, and deleting users. It also
/// supports user search with optional pagination functionality.
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UserController(
    IMapper mapper,
    IPasswordService passwordService,
    IUserRepository userRepository) : Controller
{
    /// <summary>
    /// Retrieves a paginated list of users based on the provided filters.
    /// </summary>
    /// <param name="filterDto">The filter criteria to be applied for querying users.</param>
    /// <param name="pageNumber">The page number for pagination. Must be greater than zero if pagination is used.</param>
    /// <param name="pageSize">The size of each page for pagination. Must be greater than zero if pagination is used.</param>
    /// <returns>A paginated response containing a list of users that match the provided criteria, or a bad request if invalid pagination parameters are provided.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<UserResponseDto>>> GetUsers(
        [FromQuery] UserFilterDto filterDto,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        if (pageNumber.HasValue ^ pageSize.HasValue) 
            return BadRequest(new {error = "Both pageNumber and pageSize must be provided for pagination."});
        if (pageNumber <= 0 || pageSize <= 0) 
            return BadRequest(new {error = "Both pageNumber and pageSize must be greater than zero."});

        var listings = await userRepository.GetUsersAsync(filterDto, pageNumber, pageSize);
        return Ok(new PagedResponseDto<UserResponseDto>(
            listings.ToList().Select(mapper.Map<UserResponseDto>), 
            pageSize, pageNumber));
    }

    /// <summary>
    /// Retrieves detailed information about a specific user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <returns>A response containing the user's details if found; otherwise, a not found response.</returns>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetUserById(Guid userId)
    {
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user is null)
            return NotFound();

        return Ok(mapper.Map<UserResponseDto>(user));
    }

    /// <summary>
    /// Creates a new support admin user based on the provided user data.
    /// </summary>
    /// <param name="request">The data required for creating a new user, including email, password, and user role. The role must be set to SupportAdmin.</param>
    /// <returns>An HTTP response indicating the result of the operation. Returns a 201 Created response with the created user's details when successful.
    /// Returns a 400 Bad Request if the email already exists, the role is not SupportAdmin, or the password does not meet validation criteria.</returns>
    [HttpPost]
    [Authorize(Roles = "SuperAdminOnly")]
    public async Task<ActionResult> CreateSupportAdminUser(UserRequestDto request)
    {
        if (request.Role != UserRole.SupportAdmin)
            return BadRequest(new { error = "Only support admins can be created." });

        if (await userRepository.GetUserByEmailAsync(request.Email) is not null)
            return BadRequest(new { error = "Email already exists." });

        var passwordValidation = passwordService.ValidatePasswordStrength(request.Password);
        if (passwordValidation != "")
            return BadRequest(new {error = passwordValidation});
        
        var user = mapper.Map<User>(request);
        user.Email = user.Email.ToLowerInvariant();
        user.Password = passwordService.HashPassword(request.Password);
        await userRepository.AddUserAsync(user);

        return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, mapper.Map<UserResponseDto>(user));
    }

    /// <summary>
    /// Creates a new agent user based on the provided request data.
    /// </summary>
    /// <param name="request">The request data containing email, password, and role.</param>
    /// <returns>An ActionResult representing the result of the operation, including a created response if successful,
    /// or a bad request if validation fails or the email already exists.</returns>
    [HttpPost]
    [Authorize(Roles = "SupportAdminOnly")]
    public async Task<ActionResult> CreateAgentUser(UserRequestDto request)
    {
        if (request.Role != UserRole.EstateAgent)
            return BadRequest(new {error = "Only agents can be created."});
        
        if (await userRepository.GetUserByEmailAsync(request.Email) is not null)
            return BadRequest(new {error = "Email already exists."});
        
        var passwordValidation = passwordService.ValidatePasswordStrength(request.Password);
        if (passwordValidation != "")
            return BadRequest(new {error = passwordValidation});
        
        var user = mapper.Map<User>(request);
        user.Email = user.Email.ToLowerInvariant();
        user.Password = passwordService.HashPassword(request.Password);
        await userRepository.AddUserAsync(user);

        return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, mapper.Map<UserResponseDto>(user));
    }

    /// <summary>
    /// Updates an existing user's information based on the provided request data.
    /// </summary>
    /// <param name="userId">The primary id of the <see cref="User"/> entity to be updated</param>
    /// <param name="request">The data containing the updated user information, including email, password, first name, and last name.</param>
    /// <returns>An IActionResult indicating the success or failure of the update operation.</returns>
    [HttpPut("userId:Guid")]
    public async Task<IActionResult> PutUser(Guid userId, [FromBody] UserRequestDto request)
    {
        if (await userRepository.GetUserByIdAsync(userId) is not { } existingUser)
            return NotFound();
        existingUser = mapper.Map(request, existingUser);
        await userRepository.UpdateUserAsync(existingUser);
        return Ok(mapper.Map<UserResponseDto>(existingUser));   
    }

    /// <summary>
    /// Deletes a user with the specified unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to be deleted.</param>
    /// <returns>No content if the user is successfully deleted, or NotFound if the user does not exist.</returns>
    [HttpDelete]
    public async Task<ActionResult> DeleteUser(Guid userId)
    {
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user is null) 
            return NotFound();
        
        await userRepository.DeleteUserAsync(user);
        return NoContent();
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized();
        if (await userRepository.GetUserByIdAsync(userId) is not { } user)
            return NotFound();

        if (!passwordService.VerifyPassword(request.OldPassword, user.Password))
            return BadRequest("Incorrect password.");
        
        user.Password = passwordService.HashPassword(request.NewPassword);
        await userRepository.UpdateUserAsync(user);
        return Ok();
    }
}