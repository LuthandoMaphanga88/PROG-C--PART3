namespace Techmove.Services;

/// <summary>
/// Validates currency-related data.
/// </summary>
public class CurrencyValidator
{
    private const decimal MinimumAmount = 0m;
    private const decimal MaximumAmount = decimal.MaxValue;
    private static readonly HashSet<string> ValidCurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "ZAR", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "CNY", "SEK", "NZD"
    };

    /// <summary>
    /// Validates if a currency amount is valid.
    /// </summary>
    /// <param name="amount">The amount to validate.</param>
    /// <returns>A tuple indicating if the amount is valid and an error message if invalid.</returns>
    public (bool IsValid, string? ErrorMessage) ValidateCurrencyAmount(decimal amount)
    {
        if (amount < MinimumAmount)
        {
            return (false, "Currency amount cannot be negative.");
        }

        if (amount > MaximumAmount)
        {
            return (false, "Currency amount exceeds maximum allowed value.");
        }

        return (true, null);
    }

    /// <summary>
    /// Validates if a currency code is valid.
    /// </summary>
    /// <param name="currencyCode">The currency code to validate (e.g., "USD", "ZAR").</param>
    /// <returns>A tuple indicating if the currency code is valid and an error message if invalid.</returns>
    public (bool IsValid, string? ErrorMessage) ValidateCurrencyCode(string? currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            return (false, "Currency code is required.");
        }

        var code = currencyCode.Trim().ToUpperInvariant();

        if (code.Length != 3)
        {
            return (false, "Currency code must be exactly 3 characters.");
        }

        if (!code.All(char.IsLetter))
        {
            return (false, "Currency code must contain only letters.");
        }

        if (!ValidCurrencyCodes.Contains(code))
        {
            return (false, $"Currency code '{code}' is not supported.");
        }

        return (true, null);
    }

    /// <summary>
    /// Validates if an exchange rate is valid.
    /// </summary>
    /// <param name="rate">The exchange rate to validate.</param>
    /// <returns>A tuple indicating if the rate is valid and an error message if invalid.</returns>
    public (bool IsValid, string? ErrorMessage) ValidateExchangeRate(decimal rate)
    {
        if (rate <= 0)
        {
            return (false, "Exchange rate must be greater than zero.");
        }

        if (rate > 1000000) // Reasonable upper bound for exchange rates
        {
            return (false, "Exchange rate seems unusually high.");
        }

        return (true, null);
    }

    /// <summary>
    /// Validates if a converted amount matches the expected result based on the rate.
    /// </summary>
    /// <param name="originalAmount">The original amount.</param>
    /// <param name="rate">The exchange rate.</param>
    /// <param name="convertedAmount">The converted amount.</param>
    /// <param name="tolerance">The decimal places tolerance (default 2).</param>
    /// <returns>A tuple indicating if the conversion is valid and an error message if invalid.</returns>
    public (bool IsValid, string? ErrorMessage) ValidateCurrencyConversion(
        decimal originalAmount,
        decimal rate,
        decimal convertedAmount,
        int tolerance = 2)
    {
        var (amountValid, amountError) = ValidateCurrencyAmount(originalAmount);
        if (!amountValid)
        {
            return (false, amountError);
        }

        var (rateValid, rateError) = ValidateExchangeRate(rate);
        if (!rateValid)
        {
            return (false, rateError);
        }

        var (convertedValid, convertedError) = ValidateCurrencyAmount(convertedAmount);
        if (!convertedValid)
        {
            return (false, convertedError);
        }

        var expectedAmount = originalAmount * rate;
        var difference = Math.Abs(expectedAmount - convertedAmount);
        var allowedDifference = (decimal)Math.Pow(0.1, tolerance);

        if (difference > allowedDifference)
        {
            return (false, $"Converted amount does not match expected result. Expected: {expectedAmount:F2}, Got: {convertedAmount:F2}");
        }

        return (true, null);
    }
}
