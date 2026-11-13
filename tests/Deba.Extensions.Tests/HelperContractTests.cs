using System.ComponentModel;
using System.Globalization;
using Xunit;

namespace Deba.Extensions.Tests;

public sealed class HelperContractTests
{
    [Theory]
    [InlineData(2024, 2, 29)]
    [InlineData(2025, 2, 28)]
    [InlineData(2025, 12, 31)]
    public void Month_boundaries_preserve_leap_year_and_year_end(int year, int month, int lastDay)
    {
        var date = new DateTime(year, month, 15, 14, 30, 0);

        Assert.Equal(new DateTime(year, month, 1), date.GetFirstDay());
        Assert.Equal(new DateTime(year, month, lastDay), date.GetLastDay());
    }

    [Fact]
    public void Month_boundaries_accept_explicit_year_and_month()
    {
        var date = new DateTime(2025, 7, 15);

        Assert.Equal(new DateTime(2024, 2, 1), date.GetFirstDay(year: 2024, month: 2));
        Assert.Equal(new DateTime(2024, 2, 29), date.GetLastDay(year: 2024, month: 2));
    }

    [Fact]
    public void Brazilian_date_format_does_not_depend_on_current_culture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.Equal("03/02/2024", new DateTime(2024, 2, 3).ToPtBr());
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void Text_helpers_preserve_unicode_digit_and_ASCII_filter_contracts()
    {
        Assert.Equal("Sao Joao - Acao", "São João - Ação".Unaccent());
        Assert.Equal("١23", "A١2-3 B".OnlyNumbers());
        Assert.Equal("SoJoo123", "São João ١123!".OnlyAlphanumeric());
    }

    [Fact]
    public void Enum_helpers_accept_description_or_name_case_insensitively()
    {
        Assert.Equal(State.Active, EnumExtensions.GetValue<State>("ATIVO"));
        Assert.Equal(State.Active, EnumExtensions.GetValue<State>("active"));
        Assert.True("ativo".IsEqualTo(State.Active));
        Assert.Equal("Ativo", State.Active.GetDescription());
        Assert.Equal("9", ((State)9).GetDescription());
        Assert.Equal(new[] { "None", "Ativo" }, EnumExtensions.GetAllDescriptions<State>());
        Assert.Equal(1, State.Active.GetValue());
    }

    [Fact]
    public void Enum_helper_throws_for_unknown_description()
    {
        var error = Assert.Throws<ArgumentException>(() => EnumExtensions.GetValue<State>("invalid"));

        Assert.Equal("enumDescription", error.ParamName);
    }

    [Fact]
    public void Nullable_enum_is_positive_only_when_present_and_above_zero()
    {
        Assert.False(((State?)null).HasValueGreaterThanZero());
        Assert.False(((State?)State.None).HasValueGreaterThanZero());
        Assert.True(((State?)State.Active).HasValueGreaterThanZero());
    }

    public enum State
    {
        None,
        [Description("Ativo")]
        Active
    }
}
