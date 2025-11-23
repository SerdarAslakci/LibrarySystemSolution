namespace LibrarySystem.API.Dtos.LoanDtos
{
    public class UpdateLoanDto
    {
        public int LoanId { get; set; }
        public DateTime NewExpectedReturnDate { get; set; }
    }

}
