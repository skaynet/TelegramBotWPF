using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Models
{
    public class ExchangeRates
    {
        public class ExchangeRate
        {
            public string? Currency { get; set; }
            public string? BaseCurrency { get; set; }
            public decimal SaleRateNB { get; set; }
            public decimal PurchaseRateNB { get; set; }
            public decimal SaleRate { get; set; }
            public decimal PurchaseRate { get; set; }
        }

        public string? Date { get; set; }
        public string? Bank { get; set; }
        public int BaseCurrency { get; set; }
        public string? baseCurrencyLit { get; set; }
        public List<ExchangeRate>? exchangeRate { get; set; }
    }
}
