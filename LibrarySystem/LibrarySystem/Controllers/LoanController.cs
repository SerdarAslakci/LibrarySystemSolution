using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.Dtos.LoanDtos;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibrarySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [Authorize]
        [HttpGet("my-loans")]
        public async Task<IActionResult> GetMyLoans()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }

            try
            {
                var loans = await _loanService.GetAllLoansByUserAsync(userId);

                return Ok(loans);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ödünç kayıtları alınırken beklenmedik bir hata oluştu.");
            }
        }

        [Authorize]
        [HttpGet("can-borrow")]
        public async Task<ActionResult<bool>> CanUserBorrow()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Kullanıcı bilgisi bulunamadı.");

            try
            {
                var result = await _loanService.CanUserBorrowAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Loan>> CreateLoan([FromBody] CreateLoanDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Kullanıcı bilgisi bulunamadı.");

            try
            {
                var loan = await _loanService.CreateLoanAsync(userId, dto);
                return CreatedAtAction(nameof(GetLoanById), new { id = loan.LoanId }, loan);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoanById(int id)
        {
            if (id <= 0)
                return BadRequest("Geçersiz Loan ID.");

            try
            {
                var loan = await _loanService.GetLoanByIdAsync(id);
                if (loan == null)
                    return NotFound($"Loan ID {id} bulunamadı.");

                return Ok(loan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("update-loan")]
        public async Task<ActionResult<Loan>> UpdateLoan([FromBody] UpdateLoanDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Kullanıcı bilgisi bulunamadı.");

                var updatedLoan = await _loanService.UpdateLoanAsync(dto);
                return Ok(updatedLoan);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [Authorize]
        [HttpPost("return-book")]
        public async Task<ActionResult<Loan>> ReturnBook([FromBody] ReturnBookDto returnBookDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(returnBookDto.Barcode))
                return BadRequest("Barkod numarası gereklidir.");

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Kullanıcı bilgisi bulunamadı.");

                var loanSummaryDto = await _loanService.ReturnBookAsync(returnBookDto.Barcode);

                return Ok(loanSummaryDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "İade işlemi sırasında beklenmeyen bir hata oluştu.", details = ex.Message });
            }

        }
    }
}
