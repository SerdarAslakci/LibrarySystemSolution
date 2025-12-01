namespace LibrarySystem.API.Dtos.LoanDtos
{
    public class LoanInfo
    {
        public DateTime LoanDate { get; set; }
        public DateTime ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public string BookTitle { get; set; }
        public string BarcodeNumber { get; set; }
    }
}
