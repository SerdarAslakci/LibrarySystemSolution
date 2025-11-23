namespace LibrarySystem.API.Dtos.LoanDtos
{
    public class ReturnSummaryDto
    {
        public int LoanId { get; set; }
        public string BookTitle { get; set; }
        public string Barcode { get; set; }

        public string MemberFullName { get; set; }
        public string MemberPhone { get; set; }

        public string ReturnStatus { get; set; }
        public DateTime ReturnedDate { get; set; }
        public string Message { get; set; }
    }
}
