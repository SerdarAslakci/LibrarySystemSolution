namespace LibrarySystem.API.Dtos.UserDtos
{
    public class UserFineDto
    {
        public int FineId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }

        public string BookTitle { get; set; }
        public string BarcodeNumber { get; set; }

        public string FineType { get; set; }
    }
}
