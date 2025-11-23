using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem.Models.Models
{
    public class Loan
    {
        public int Id { get; set; }

        [ForeignKey("AppUser")]
        public string UserId { get; set; }
        public int BookCopyId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public AppUser AppUser { get; set; }
        public BookCopy BookCopy { get; set; }
    }
}
