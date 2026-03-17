using BankMore.Account.Domain.ValueObjects;
using BankMore.BuildingBlocks.Domain.Common;
using FluentAssertions;

namespace BankMore.Account.UnitTests.Domain.ValueObjects;

public sealed class CpfTests
{
    [Fact]
    public void Create_Should_Create_Cpf_When_Valid()
    {
        var cpf = Cpf.Create("529.982.247-25");

        cpf.Value.Should().Be("52998224725");
    }

    [Fact]
    public void Create_Should_Throw_When_Invalid()
    {
        var act = () => Cpf.Create("11111111111");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TryCreate_Should_Return_True_When_Valid()
    {
        var success = Cpf.TryCreate("52998224725", out var cpf);

        success.Should().BeTrue();
        cpf.Should().NotBeNull();
        cpf!.Value.Should().Be("52998224725");
    }

    [Fact]
    public void TryCreate_Should_Return_False_When_Invalid()
    {
        var success = Cpf.TryCreate("123", out var cpf);

        success.Should().BeFalse();
        cpf.Should().BeNull();
    }

    [Fact]
    public void Normalize_Should_Remove_NonDigits()
    {
        var normalized = Cpf.Normalize("529.982.247-25");

        normalized.Should().Be("52998224725");
    }
}