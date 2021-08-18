using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TraderBot.Models
{
    public class Symbol
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<TimeSeries> TimeSeries { get; set; }
    }
}
