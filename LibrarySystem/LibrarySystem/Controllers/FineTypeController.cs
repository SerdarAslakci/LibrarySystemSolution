using LibrarySystem.API.Dtos.FineTypeDtos;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FineTypeController : ControllerBase
    {
        private readonly IFineTypeService _fineTypeService;

        public FineTypeController(IFineTypeService fineTypeService)
        {
            _fineTypeService = fineTypeService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllFineTypes()
        {
            var fineTypes = await _fineTypeService.GetAllFineTypesAsync();
            return Ok(fineTypes);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var fineType = await _fineTypeService.GetByIdAsync(id);
                return Ok(fineType);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddFineType([FromBody] CreateFineTypeDto fineTypeDto)
        {
            if (fineTypeDto == null)
                return BadRequest(new { message = "Ceza tipi boş olamaz." });

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                
                var added = await _fineTypeService.AddFineTypeAsync(fineTypeDto);
                return CreatedAtAction(nameof(GetById), new { id = added.Id }, added);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Ceza tipi eklenirken bir hata oluştu." });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateFineType([FromBody] UpdateFineTypeDto fineType)
        {
            if (fineType == null)
                return BadRequest(new { message = "Geçersiz ceza tipi" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _fineTypeService.UpdateFineTypeAsync(fineType);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Ceza tipi güncellenirken bir hata oluştu." });
            }
        }
    }
}
