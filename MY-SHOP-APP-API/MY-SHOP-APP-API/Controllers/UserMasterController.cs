using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using MY_SHOP_APP_API.Business;
using MY_SHOP_APP_API.Models;

namespace MY_SHOP_APP_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserMasterController : ControllerBase
    {
        private readonly IUserMasterService _service;

        public UserMasterController(IUserMasterService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<MY_SHOP_APP_API.Models.PagedResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] bool onlyActive = true)
        {
            var result = await _service.GetAllAsync(pageNumber, pageSize, search, onlyActive);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>> Get(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<MY_SHOP_APP_API.Models.DTOs.UserMasterDto>> Create([FromBody] MY_SHOP_APP_API.Models.DTOs.CreateUserMasterDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateAsync(model);
            if (!result.Success)
            {
                return Conflict(new { message = result.Message });
            }

            var created = result.Data!;
            return CreatedAtAction(nameof(Get), new { id = created.UserId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MY_SHOP_APP_API.Models.DTOs.UpdateUserMasterDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, model);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
