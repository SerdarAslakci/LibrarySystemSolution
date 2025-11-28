namespace LibrarySystem.API.Dtos.DashboardDtos
{
    public class DashboardDto
    {
        public int TotalBookCount { get; set; }
        public int UserCount { get; set; }
        public int LoanedBookCount { get; set; }
        public int OverdueLoanCount { get; set; }
    }
}
