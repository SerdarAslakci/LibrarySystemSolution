using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem.Models.Models
{
    public class Shelf
    {
        public int Id { get; set; }
        public string? ShelfCode { get; set; }
        public int RoomId { get; set; }
        public Room? Room { get; set; }
    }
}
