using LibrarySystem.API.Dtos.LoanDtos;

namespace LibrarySystem.API.Dtos.UserDtos
{
    public class UserFineDto
    {
        public int FineId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime IssuedDate { get; set; }
        public string FineType { get; set; }

        public LoanInfo? LoanDetails { get; set; }
    }
}
