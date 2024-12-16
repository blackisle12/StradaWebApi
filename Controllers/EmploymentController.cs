using Microsoft.AspNetCore.Mvc;
using Strada.Repository.Models;
using Strada.Service.Interface;

namespace StradaWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmploymentController : ControllerBase
    {
        private readonly IEmploymentService _employmentService;
        private readonly IUserService _userService;
        private readonly ILogger<EmploymentController> _logger;

        public EmploymentController(
            IEmploymentService employmentService,
            IUserService userService,
            ILogger<EmploymentController> logger)
        {
            _employmentService = employmentService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var employment = await _employmentService.GetAsync(id);

                if (employment is null)
                {
                    return NotFound();
                }

                return Ok(employment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Employment employment)
        {
            try
            {
                if (employment == null)
                {
                    return BadRequest("Employment data is required");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!_userService.EmploymentsAreValid(new List<Employment> { employment }, out var validationErrors))
                {
                    return BadRequest(string.Join(", ", validationErrors));
                }

                var user = await _userService.GetAsync(employment.UserId);

                if (user == null)
                {
                    return BadRequest($"User with id of {employment.UserId} does not exists");
                }

                var createdEmployment = await _employmentService.CreateAsync(employment);

                return CreatedAtAction(nameof(GetById), new { id = createdEmployment.Id }, createdEmployment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Employment employment)
        {
            try
            {
                if (employment == null)
                {
                    return BadRequest("Employment data is required");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!_userService.EmploymentsAreValid(new List<Employment> {  employment }, out var validationErrors))
                {
                    return BadRequest(string.Join(", ", validationErrors));
                }

                var user = await _userService.GetAsync(employment.UserId);

                if (user == null)
                {
                    return BadRequest($"User with id of {employment.UserId} does not exists");
                }

                var updated = await _employmentService.UpdateAsync(id, employment);

                if (!updated)
                {
                    return NotFound($"Employment with id of {id} does not exists");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _employmentService.DeleteAsync(id);

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
