using BankMore.BuildingBlocks.Domain.Common;
using BankMore.Transfer.Domain.Entities;
using FluentAssertions;

namespace BankMore.Transfer.UnitTests.Domain.Entities;

public sealed class TransferOperationTests
{
    [Fact]
    public void Create_Should_Create_Transfer_When_Valid()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var destinationId = Guid.NewGuid();

        // Act
        var transfer = TransferOperation.Create(
            requestId: "transfer-001",
            sourceAccountId: sourceId,
            destinationAccountId: destinationId,
            amount: 150.75m);

        // Assert
        transfer.RequestId.Should().Be("transfer-001");
        transfer.SourceAccountId.Should().Be(sourceId);
        transfer.DestinationAccountId.Should().Be(destinationId);
        transfer.Amount.Should().Be(150.75m);
    }

    [Fact]
    public void Create_Should_Throw_When_Value_Is_Invalid()
    {
        // Act
        var act = () => TransferOperation.Create(
            requestId: "transfer-001",
            sourceAccountId: Guid.NewGuid(),
            destinationAccountId: Guid.NewGuid(),
            amount: 0);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Should_Throw_When_Source_And_Destination_Are_The_Same()
    {
        // Arrange
        var sameId = Guid.NewGuid();

        // Act
        var act = () => TransferOperation.Create(
            requestId: "transfer-001",
            sourceAccountId: sameId,
            destinationAccountId: sameId,
            amount: 100);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Should_Throw_When_RequestId_Is_Empty()
    {
        // Act
        var act = () => TransferOperation.Create(
            requestId: "",
            sourceAccountId: Guid.NewGuid(),
            destinationAccountId: Guid.NewGuid(),
            amount: 100);

        // Assert
        act.Should().Throw<DomainException>();
    }
}