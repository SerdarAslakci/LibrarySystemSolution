namespace LibrarySystem.API.Dtos.UserDtos
{
    public class UserStatsDto
    {
        public int ActiveLoanCount { get; set; }
        public int TotalReadCount { get; set; }
        public decimal TotalFineDebt { get; set; }
    }
}
