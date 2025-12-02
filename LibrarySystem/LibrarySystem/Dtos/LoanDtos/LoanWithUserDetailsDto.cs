namespace LibrarySystem.API.Dtos.LoanDtos
{
    public class LoanWithUserDetailsDto
    {
        public int LoanId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public bool IsActive => ActualReturnDate == null;

        public string BookTitle { get; set; }
        public string AuthorName { get; set; }
        public string Isbn { get; set; }

        public string Room { get; set; }
        public string Shelf { get; set; }

        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhoneNumber { get; set; }
    }
}
