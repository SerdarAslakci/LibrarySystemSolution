using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem.Models.Models
{
    public class Fine
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int? LoanId { get; set; }
        public int FineTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime IssuedDate { get; set; }
        public Loan? Loan { get; set; }
        public FineType FineType { get; set; }
    }
}
