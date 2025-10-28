using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class StatisticalModel
    {
        [Key]
        public int Id { get; set; }
        public decimal revenue { get; set; }
        public int orders { get; set; }
        public DateTime date { get; set; }

    }
}
