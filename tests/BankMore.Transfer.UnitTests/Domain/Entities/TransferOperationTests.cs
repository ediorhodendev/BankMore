using BankMore.BuildingBlocks.Domain.Common;
using BankMore.Transfer.Domain.Entities;
using FluentAssertions;

namespace BankMore.Transfer.UnitTests.Domain.Entities;

public sealed class TransferOperationTests
{
    [Fact]
    public void Create_Should_Create_Transfer_When_Valid()
    {
        var sourceId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();

        var transfer = TransferOperation.Create(
            requestId: "transfer-001",
            sourceAccountId: sourceId,
            sourceAccountNumber: "1111111111",
            destinationAccountId: destinationId,
            destinationAccountNumber: "2222222222",
            amount: 150.75m);

        transfer.RequestId.Should().Be("transfer-001");
        transfer.SourceAccountId.Should().Be(sourceId);
        transfer.SourceAccountNumber.Should().Be("1111111111");
        transfer.DestinationAccountId.Should().Be(destinationId);
        transfer.DestinationAccountNumber.Should().Be("2222222222");
        transfer.Amount.Should().Be(150.75m);
    }

    [Fact]
    public void Create_Should_Throw_When_Value_Is_Invalid()
    {
        var act = () => TransferOperation.Create(
            requestId: "transfer-001",
            sourceAccountId: Guid.NewGuid(),
            sourceAccountNumber: "1111111111",
            destinationAccountId: Guid.NewGuid(),
            destinationAccountNumber: "2222222222",
            amount: 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Should_Throw_When_Source_And_Destination_Are_The_Same()
    {
        var sameId = Guid.NewGuid();

        var act = () => TransferOperation.Create(
            requestId: "transfer-001",
            sourceAccountId: sameId,
            sourceAccountNumber: "1111111111",
            destinationAccountId: sameId,
            destinationAccountNumber: "1111111111",
            amount: 100);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Should_Throw_When_RequestId_Is_Empty()
    {
        var act = () => TransferOperation.Create(
            requestId: "",
            sourceAccountId: Guid.NewGuid(),
            sourceAccountNumber: "1111111111",
            destinationAccountId: Guid.NewGuid(),
            destinationAccountNumber: "2222222222",
            amount: 100);

        act.Should().Throw<DomainException>();
    }
}