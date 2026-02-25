using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace ExpenseTracker.Services
{
    public static class CurrencyService
    {
        public static string CurrentCurrency
        {
            get => Preferences.Get("AppCurrency", "VND");
            set => Preferences.Set("AppCurrency", value);
        }

        public static Dictionary<string, CurrencyInfo> Currencies = new()
        {
            ["VND"] = new CurrencyInfo { Code = "VND", Symbol = "₫", Name = "Vietnamese Dong", Format = "{0:N0} ₫" },
            ["USD"] = new CurrencyInfo { Code = "USD", Symbol = "$", Name = "US Dollar", Format = "${0:N2}" },
            ["EUR"] = new CurrencyInfo { Code = "EUR", Symbol = "€", Name = "Euro", Format = "€{0:N2}" }
        };

        public static string FormatAmount(decimal amount)
        {
            var currency = CurrentCurrency;
            if (Currencies.ContainsKey(currency))
            {
                return string.Format(Currencies[currency].Format, amount);
            }
            return $"{amount:N0} ₫";
        }

        public static string GetSymbol()
        {
            var currency = CurrentCurrency;
            if (Currencies.ContainsKey(currency))
            {
                return Currencies[currency].Symbol;
            }
            return "₫";
        }

        public static void SetCurrency(string currencyCode)
        {
            if (Currencies.ContainsKey(currencyCode))
            {
                CurrentCurrency = currencyCode;
            }
        }
    }

    public class CurrencyInfo
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
    }
}