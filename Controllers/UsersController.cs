using Microsoft.AspNetCore.Mvc;
using Strada.Repository.Models;
using Strada.Service.Interface;

namespace StradaWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            ILogger<UserController> logger)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _userService.GetAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _userService.GetAsync(id);

                if (user is null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest("User data is required");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!_userService.EmploymentsAreValid(user.Employments, out var validationErrors))
                {
                    return BadRequest(string.Join(", ", validationErrors));
                }

                if (await _userService.IsExistingAsync(user.Email))
                {
                    return BadRequest($"A user wil email {user.Email} already exists.");
                }

                var createdUser = await _userService.CreateAsync(user);

                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest("User data is required");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!_userService.EmploymentsAreValid(user.Employments, out var validationErrors))
                {
                    return BadRequest(string.Join(", ", validationErrors));
                }

                if (await _userService.IsExistingAsync(user.Email, id))
                {
                    return BadRequest($"A user wil email {user.Email} already exists.");
                }

                var updated = await _userService.UpdateAsync(id, user);

                if (!updated)
                {
                    return NotFound($"User {user.Email} is invalid.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
