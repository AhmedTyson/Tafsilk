using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TafsilkPlatform.Models
{
    public class Wallet
    {
        [Key]
        public int WalletId { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }

        public decimal Balance { get; set; }

    }
}
