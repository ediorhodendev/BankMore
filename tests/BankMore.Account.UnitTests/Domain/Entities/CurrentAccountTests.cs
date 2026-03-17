using BankMore.Account.Domain.Entities;
using BankMore.BuildingBlocks.Domain.Common;
using FluentAssertions;

namespace BankMore.Account.UnitTests.Domain.Entities;

public sealed class CurrentAccountTests
{
    [Fact]
    public void Create_Should_Create_Active_Account()
    {
        var account = CurrentAccount.Create(
            accountNumber: "1234567890",
            name: "Edio Rhoden",
            cpf: "52998224725",
            passwordHash: "HASH");

        account.AccountNumber.Should().Be("1234567890");
        account.Name.Should().Be("Edio Rhoden");
        account.Cpf.Should().Be("52998224725");
        account.PasswordHash.Should().Be("HASH");
        account.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_Should_Deactivate_Account()
    {
        var account = CurrentAccount.Create(
            accountNumber: "1234567890",
            name: "Edio Rhoden",
            cpf: "52998224725",
            passwordHash: "HASH");

        account.Deactivate();

        account.IsActive.Should().BeFalse();
        account.DeactivatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_Should_Throw_When_Already_Inactive()
    {
        var account = CurrentAccount.Create(
            accountNumber: "1234567890",
            name: "Edio Rhoden",
            cpf: "52998224725",
            passwordHash: "HASH");

        account.Deactivate();

        var act = () => account.Deactivate();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void HasCpf_Should_Return_True_When_Matching()
    {
        var account = CurrentAccount.Create(
            accountNumber: "1234567890",
            name: "Edio Rhoden",
            cpf: "52998224725",
            passwordHash: "HASH");

        var result = account.HasCpf("529.982.247-25");

        result.Should().BeTrue();
    }

    [Fact]
    public void HasAccountNumber_Should_Return_True_When_Matching()
    {
        var account = CurrentAccount.Create(
            accountNumber: "1234567890",
            name: "Edio Rhoden",
            cpf: "52998224725",
            passwordHash: "HASH");

        var result = account.HasAccountNumber("1234567890");

        result.Should().BeTrue();
    }
}