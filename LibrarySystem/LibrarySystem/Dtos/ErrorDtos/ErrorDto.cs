namespace LibrarySystem.API.Dtos.ErrorDtos
{
    public class ErrorDto
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
