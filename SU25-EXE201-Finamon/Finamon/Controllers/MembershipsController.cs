using Finamon.Service.Interfaces;
using Finamon.Service.ReponseModel;
using Finamon.Service.RequestModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic; // Required for KeyNotFoundException
using System; // Required for InvalidOperationException

namespace Finamon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipsController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipsController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<MembershipResponse>>> GetAllMemberships([FromQuery] MembershipQueryRequest queryRequest)
        {
            var memberships = await _membershipService.GetAllMembershipsAsync(queryRequest);
            return Ok(memberships);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MembershipResponse>> GetMembershipById(int id)
        {
            try
            {
                var membership = await _membershipService.GetMembershipByIdAsync(id);
                return Ok(membership);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<MembershipResponse>> CreateMembership([FromBody] CreateMembershipRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var membership = await _membershipService.CreateMembershipAsync(request);
            return CreatedAtAction(nameof(GetMembershipById), new { id = membership.Id }, membership);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MembershipResponse>> UpdateMembership(int id, [FromBody] UpdateMembershipRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var membership = await _membershipService.UpdateMembershipAsync(id, request);
                return Ok(membership);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMembership(int id)
        {
            try
            {
                var result = await _membershipService.DeleteMembershipAsync(id);
                if (result)
                {
                    return NoContent();
                }
                // This part might be unreachable if DeleteMembershipAsync throws an exception for not found
                return NotFound(new { message = $"Membership with ID {id} not found."}); 
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Catching the specific exception for when a membership is in use
                return Conflict(new { message = ex.Message });
            }
        }
    }
} 