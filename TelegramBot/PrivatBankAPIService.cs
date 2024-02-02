using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using TelegramBot.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TelegramBot.Models.ExchangeRates;

namespace TelegramBot
{
    public class PrivatBankAPIService
    {
        public string[] SupportedCurrencies { get; init; } = { "USD", "EUR", "GBP", "AUD", "AZN", "BYN", "CAD", "CHF", "CNY", "CZK", "DKK", "GEL", "HUF", "ILS", "JPY", "KZT", "MDL", "NOK", "PLN", "SEK", "SGD", "TMT", "TRY", "UZS", "XAU" };
        
        private readonly string _urlApi = "https://api.privatbank.ua/p24api/exchange_rates?date=";
        private Dictionary<string, ExchangeRates> _cachExchangeRates;

        public PrivatBankAPIService()
        {
            _cachExchangeRates = new Dictionary<string, ExchangeRates>();
        }

        public async Task<ExchangeRates?> RequestAsync(string? date = null)
        {
            if (string.IsNullOrEmpty(date))
            {
                date = GetCurrentDateStr();
            }

            ExchangeRates? exchangeRatesResponse = GetCachExchangeRates(date);
            if (exchangeRatesResponse != null)
            {
                return exchangeRatesResponse;
            }

            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync($"{_urlApi}{date}");
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    exchangeRatesResponse = JsonSerializer.Deserialize<ExchangeRates>(jsonResponse, options);
                    if (exchangeRatesResponse != null)
                    {
                        _cachExchangeRates.Add(date, exchangeRatesResponse);
                        return exchangeRatesResponse;
                    }
                }
                return null;
            }
        }

        public bool DateCheck(string dateStr)
        {
            try
            {
                DateTime date = DateTime.ParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                return DateTime.Compare(date, DateTime.Now) <= 0;
            }
            catch { return false; }
        }

        public string GetCurrentDateStr()
        {
            return DateTime.Now.ToString("dd.MM.yyyy");
        }

        private ExchangeRates? GetCachExchangeRates(string date)
        {
            if (_cachExchangeRates.TryGetValue(date, out ExchangeRates? exchangeRates))
            {
                return exchangeRates;
            }
            return null;
        }
    }
}
