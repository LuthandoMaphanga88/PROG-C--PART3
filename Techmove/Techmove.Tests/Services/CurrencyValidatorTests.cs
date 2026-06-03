using Techmove.Services;
using Xunit;

namespace Techmove.Tests.Services;

public class CurrencyValidatorTests
{
    private readonly CurrencyValidator _currencyValidator = new();

    #region ValidateCurrencyAmount Tests

    [Fact]
    public void ValidateCurrencyAmount_WithNegativeAmount_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyAmount(-10m);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Currency amount cannot be negative.", errorMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(100.50)]
    [InlineData(1000000)]
    [InlineData(999999999999)]
    public void ValidateCurrencyAmount_WithValidAmount_ReturnsTrue(decimal amount)
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyAmount(amount);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    #endregion

    #region ValidateCurrencyCode Tests

    [Fact]
    public void ValidateCurrencyCode_WithNullCode_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyCode(null);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Currency code is required.", errorMessage);
    }

    [Fact]
    public void ValidateCurrencyCode_WithEmptyCode_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyCode(string.Empty);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Currency code is required.", errorMessage);
    }

    [Fact]
    public void ValidateCurrencyCode_WithWhitespaceCode_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyCode("   ");

        // Assert
        Assert.False(isValid);
        Assert.Equal("Currency code is required.", errorMessage);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDA")]
    [InlineData("U")]
    public void ValidateCurrencyCode_WithIncorrectLength_ReturnsFalseAndErrorMessage(string code)
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyCode(code);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Currency code must be exactly 3 characters.", errorMessage);
    }

    [Theory]
    [InlineData("1SD")]
    [InlineData("US1")]
    [InlineData("U-D")]
    public void ValidateCurrencyCode_WithNonLetterCharacters_ReturnsFalseAndErrorMessage(string code)
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyCode(code);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Currency code must contain only letters.", errorMessage);
    }

    [Fact]
    public void ValidateCurrencyCode_WithUnsupportedCode_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyCode("XYZ");

        // Assert
        Assert.False(isValid);
        Assert.Contains("not supported", errorMessage);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("ZAR")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("usd")] 
    [InlineData("eur")] 
    [InlineData("gbp")] 
    public void ValidateCurrencyCode_WithValidCode_ReturnsTrue(string code)
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyCode(code);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Theory]
    [InlineData(" USD ")]
    [InlineData(" eur ")]
    public void ValidateCurrencyCode_WithValidCodeAndWhitespace_ReturnsTrue(string code)
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyCode(code);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    #endregion

    #region ValidateExchangeRate Tests

    [Fact]
    public void ValidateExchangeRate_WithZeroRate_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateExchangeRate(0);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Exchange rate must be greater than zero.", errorMessage);
    }

    [Fact]
    public void ValidateExchangeRate_WithNegativeRate_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateExchangeRate(-5.5m);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Exchange rate must be greater than zero.", errorMessage);
    }

    [Fact]
    public void ValidateExchangeRate_WithExcessivelyHighRate_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateExchangeRate(5000000);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Exchange rate seems unusually high.", errorMessage);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.0)]
    [InlineData(18.50)] // ZAR to USD
    [InlineData(0.92)] // USD to EUR
    [InlineData(0.79)] // USD to GBP
    [InlineData(999999)] // Just under the limit
    public void ValidateExchangeRate_WithValidRate_ReturnsTrue(decimal rate)
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateExchangeRate(rate);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    #endregion

    #region ValidateCurrencyConversion Tests

    [Fact]
    public void ValidateCurrencyConversion_WithNegativeOriginalAmount_ReturnsFalse()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyConversion(-100, 18.5m, 1850);

        // Assert
        Assert.False(isValid);
        Assert.Contains("negative", errorMessage);
    }

    [Fact]
    public void ValidateCurrencyConversion_WithInvalidRate_ReturnsFalse()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyConversion(100, 0, 1850);

        // Assert
        Assert.False(isValid);
        Assert.Contains("greater than zero", errorMessage);
    }

    [Fact]
    public void ValidateCurrencyConversion_WithNegativeConvertedAmount_ReturnsFalse()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyConversion(100, 18.5m, -1850);

        // Assert
        Assert.False(isValid);
        Assert.Contains("negative", errorMessage);
    }

    [Theory]
    [InlineData(100, 1.0, 100.00)] // 1:1 rate
    [InlineData(100, 18.5, 1850.00)] // USD to ZAR
    [InlineData(100, 0.92, 92.00)] // USD to EUR
    [InlineData(50, 0.79, 39.50)] // USD to GBP
    [InlineData(1000, 0.05, 50.00)]
    public void ValidateCurrencyConversion_WithValidConversion_ReturnsTrue(decimal original, decimal rate, decimal converted)
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyConversion(original, rate, converted);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateCurrencyConversion_WithSmallRoundingDifference_ReturnsTrue()
    {
        // Arrange
        decimal original = 100;
        decimal rate = 18.567m;
        decimal converted = 1856.70m; // Slight rounding difference

        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyConversion(original, rate, converted, tolerance: 2);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateCurrencyConversion_WithLargeRoundingDifference_ReturnsFalse()
    {
        // Arrange
        decimal original = 100;
        decimal rate = 18.5m;
        decimal converted = 2000.00m; // Significantly different

        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyConversion(original, rate, converted);

        // Assert
        Assert.False(isValid);
        Assert.Contains("does not match", errorMessage);
    }

    [Fact]
    public void ValidateCurrencyConversion_WithDifferentTolerances_ValidatesCorrectly()
    {
        // Arrange
        decimal original = 100;
        decimal rate = 18.567m;
        decimal converted = 1856.700m;

        // Act - tolerance of 3 decimal places should pass
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyConversion(original, rate, converted, tolerance: 3);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void ValidateCurrencyConversion_WithZeroAmount_ReturnsTrue()
    {
        // Act
        var (isValid, errorMessage) = _currencyValidator.ValidateCurrencyConversion(0, 18.5m, 0);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    #endregion
}
