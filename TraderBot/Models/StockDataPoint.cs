using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderBot.Models
{
    public class StockDataPoint
    {
        [Key]
        public long Id { get; set; }

        public int TimeSeriesId { get; set; }

        public virtual TimeSeries TimeSeries { get; set; }

        public decimal ClosingPrice { get; set; }

        public decimal OpeningPrice { get; set; }

        public decimal LowestPrice { get; set; }

        public decimal HighestPrice { get; set; }

        public DateTime Time { get; set; }

        public long Volume { get; set; }
    }
}
