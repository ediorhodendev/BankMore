using BankMore.Account.Domain.Entities;
using BankMore.Account.Domain.Enums;
using BankMore.BuildingBlocks.Domain.Common;
using FluentAssertions;

namespace BankMore.Account.UnitTests.Domain.Entities;

public sealed class MovementTests
{
    [Fact]
    public void Create_Should_Create_Credit_Movement()
    {
        var movement = Movement.Create(
            currentAccountId: Guid.NewGuid(),
            requestId: "req-1",
            type: MovementType.Credit,
            amount: 100);

        movement.IsCredit().Should().BeTrue();
        movement.IsDebit().Should().BeFalse();
        movement.SignedAmount().Should().Be(100);
    }

    [Fact]
    public void Create_Should_Create_Debit_Movement()
    {
        var movement = Movement.Create(
            currentAccountId: Guid.NewGuid(),
            requestId: "req-1",
            type: MovementType.Debit,
            amount: 100);

        movement.IsCredit().Should().BeFalse();
        movement.IsDebit().Should().BeTrue();
        movement.SignedAmount().Should().Be(-100);
    }

    [Fact]
    public void Create_Should_Throw_When_Amount_Is_Invalid()
    {
        var act = () => Movement.Create(
            currentAccountId: Guid.NewGuid(),
            requestId: "req-1",
            type: MovementType.Credit,
            amount: 0);

        act.Should().Throw<DomainException>();
    }
}