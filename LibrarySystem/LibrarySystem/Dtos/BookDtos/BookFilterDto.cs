namespace LibrarySystem.API.Dtos.BookDtos
{
    public class BookFilterDto
    {
        public string? Title { get; set; }
        public int? CategoryId { get; set; }
        public int? PublicationYearFrom { get; set; }
        public int? PublicationYearTo { get; set; }
        public string? Language { get; set; }
        public int? PageCountMin { get; set; }
        public int? PageCountMax { get; set; }
        public bool? HasAvailableCopy { get; set; }
        public string? RoomCode { get; set; }
    }
}
