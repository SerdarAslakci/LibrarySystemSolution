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

        // --- Ödünç Bilgileri (İç İçe Nesne) ---
        // Eğer ceza manuel ise burası NULL olacak.
        public LoanInfo? LoanDetails { get; set; }
    }
}
