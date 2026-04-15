using BankMore.Transfer.Application.Abstractions.Persistence;
using BankMore.Transfer.Application.Abstractions.Security;
using BankMore.Transfer.Application.Abstractions.Services;
using BankMore.Transfer.Application.Features.PerformTransfer;
using BankMore.Transfer.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BankMore.Transfer.UnitTests.Application.Features.PerformTransfer;

public sealed class PerformTransferCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Transfer_With_Success()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();

        var transferRepository = new Mock<ITransferRepository>();

        var idempotency = new Mock<ITransferIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("transfer", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(sourceAccountId);
        currentUser.Setup(x => x.BearerToken).Returns("TOKEN");

        var accountApiClient = new Mock<IAccountApiClient>();
        accountApiClient
            .Setup(x => x.GetCurrentAccountAsync("TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = sourceAccountId,
                Name = "Origem",
                IsActive = true
            });

        accountApiClient
            .Setup(x => x.ResolveAccountByNumberAsync("2222222222", "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = destinationAccountId,
                Name = "Destino",
                IsActive = true
            });

        accountApiClient
            .Setup(x => x.CreateDebitAsync("req-1-debit", 100m, "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AccountMovementResult.Success());

        accountApiClient
            .Setup(x => x.CreateCreditByAccountIdAsync("req-1-credit", destinationAccountId, 100m, "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AccountMovementResult.Success());

        var publisher = new Mock<ITransferEventPublisher>();

        var handler = new Transfer.Application.Features.PerformTransfer.PerformTransferCommandHandler(
            transferRepository.Object,
            idempotency.Object,
            currentUser.Object,
            accountApiClient.Object,
            publisher.Object);

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RequestId.Should().Be("req-1");
        result.Value.SourceAccountId.Should().Be(sourceAccountId);
        result.Value.DestinationAccountId.Should().Be(destinationAccountId);
        result.Value.Amount.Should().Be(100m);

        transferRepository.Verify(
            x => x.AddAsync(It.IsAny<TransferOperation>(), It.IsAny<CancellationToken>()),
            Times.Once);

        idempotency.Verify(
            x => x.RegisterAsync("transfer", "req-1", It.IsAny<CancellationToken>()),
            Times.Once);

        publisher.Verify(
            x => x.PublishCompletedAsync(
                It.IsAny<Guid>(),
                sourceAccountId,
                destinationAccountId,
                100m,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Existing_Transfer_When_Request_Is_Duplicate()
    {
        // Arrange
        var existing = TransferOperation.Create(
            requestId: "req-1",
            sourceAccountId: Guid.NewGuid(),
            destinationAccountId: Guid.NewGuid(),
            amount: 100m);

        var transferRepository = new Mock<ITransferRepository>();
        transferRepository
            .Setup(x => x.GetByRequestIdAsync("req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var idempotency = new Mock<ITransferIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("transfer", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(Guid.NewGuid());
        currentUser.Setup(x => x.BearerToken).Returns("TOKEN");

        var accountApiClient = new Mock<IAccountApiClient>();
        var publisher = new Mock<ITransferEventPublisher>();

        var handler = new PerformTransferCommandHandler(
            transferRepository.Object,
            idempotency.Object,
            currentUser.Object,
            accountApiClient.Object,
            publisher.Object);

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.RequestId.Should().Be("req-1");
        result.Value.SourceAccountId.Should().Be(existing.SourceAccountId);
        result.Value.DestinationAccountId.Should().Be(existing.DestinationAccountId);
        result.Value.Amount.Should().Be(100m);

        accountApiClient.Verify(
            x => x.CreateDebitAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        accountApiClient.Verify(
            x => x.CreateCreditByAccountIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        publisher.Verify(
            x => x.PublishCompletedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Not_Authenticated()
    {
        // Arrange
        var handler = new PerformTransferCommandHandler(
            Mock.Of<ITransferRepository>(),
            Mock.Of<ITransferIdempotencyService>(),
            Mock.Of<ICurrentUserService>(),
            Mock.Of<IAccountApiClient>(),
            Mock.Of<ITransferEventPublisher>());

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("USER_UNAUTHORIZED");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Bearer_Token_Is_Missing()
    {
        // Arrange
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(Guid.NewGuid());
        currentUser.Setup(x => x.BearerToken).Returns((string?)null);

        var handler = new PerformTransferCommandHandler(
            Mock.Of<ITransferRepository>(),
            Mock.Of<ITransferIdempotencyService>(),
            currentUser.Object,
            Mock.Of<IAccountApiClient>(),
            Mock.Of<ITransferEventPublisher>());

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("USER_UNAUTHORIZED");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Value_Is_Invalid()
    {
        // Arrange
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(Guid.NewGuid());
        currentUser.Setup(x => x.BearerToken).Returns("TOKEN");

        var handler = new PerformTransferCommandHandler(
            Mock.Of<ITransferRepository>(),
            Mock.Of<ITransferIdempotencyService>(),
            currentUser.Object,
            Mock.Of<IAccountApiClient>(),
            Mock.Of<ITransferEventPublisher>());

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 0m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_VALUE");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Source_Account_Does_Not_Exist()
    {
        // Arrange
        var idempotency = new Mock<ITransferIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("transfer", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(Guid.NewGuid());
        currentUser.Setup(x => x.BearerToken).Returns("TOKEN");

        var accountApiClient = new Mock<IAccountApiClient>();
        accountApiClient
            .Setup(x => x.GetCurrentAccountAsync("TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountLookupResult?)null);

        var handler = new PerformTransferCommandHandler(
            Mock.Of<ITransferRepository>(),
            idempotency.Object,
            currentUser.Object,
            accountApiClient.Object,
            Mock.Of<ITransferEventPublisher>());

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_ACCOUNT");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Source_Account_Is_Inactive()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();

        var idempotency = new Mock<ITransferIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("transfer", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(sourceAccountId);
        currentUser.Setup(x => x.BearerToken).Returns("TOKEN");

        var accountApiClient = new Mock<IAccountApiClient>();
        accountApiClient
            .Setup(x => x.GetCurrentAccountAsync("TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = sourceAccountId,
                Name = "Origem",
                IsActive = false
            });

        var handler = new PerformTransferCommandHandler(
            Mock.Of<ITransferRepository>(),
            idempotency.Object,
            currentUser.Object,
            accountApiClient.Object,
            Mock.Of<ITransferEventPublisher>());

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INACTIVE_ACCOUNT");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Destination_Account_Does_Not_Exist()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();

        var idempotency = new Mock<ITransferIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("transfer", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(sourceAccountId);
        currentUser.Setup(x => x.BearerToken).Returns("TOKEN");

        var accountApiClient = new Mock<IAccountApiClient>();
        accountApiClient
            .Setup(x => x.GetCurrentAccountAsync("TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = sourceAccountId,
                Name = "Origem",
                IsActive = true
            });

        accountApiClient
            .Setup(x => x.ResolveAccountByNumberAsync("2222222222", "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountLookupResult?)null);

        var handler = new PerformTransferCommandHandler(
            Mock.Of<ITransferRepository>(),
            idempotency.Object,
            currentUser.Object,
            accountApiClient.Object,
            Mock.Of<ITransferEventPublisher>());

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_ACCOUNT");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Destination_Account_Is_Inactive()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();

        var idempotency = new Mock<ITransferIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("transfer", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(sourceAccountId);
        currentUser.Setup(x => x.BearerToken).Returns("TOKEN");

        var accountApiClient = new Mock<IAccountApiClient>();
        accountApiClient
            .Setup(x => x.GetCurrentAccountAsync("TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = sourceAccountId,
                Name = "Origem",
                IsActive = true
            });

        accountApiClient
            .Setup(x => x.ResolveAccountByNumberAsync("2222222222", "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = Guid.NewGuid(),
                Name = "Destino",
                IsActive = false
            });

        var handler = new PerformTransferCommandHandler(
            Mock.Of<ITransferRepository>(),
            idempotency.Object,
            currentUser.Object,
            accountApiClient.Object,
            Mock.Of<ITransferEventPublisher>());

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INACTIVE_ACCOUNT");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Source_And_Destination_Are_The_Same()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();

        var idempotency = new Mock<ITransferIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("transfer", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(sourceAccountId);
        currentUser.Setup(x => x.BearerToken).Returns("TOKEN");

        var accountApiClient = new Mock<IAccountApiClient>();
        accountApiClient
            .Setup(x => x.GetCurrentAccountAsync("TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = sourceAccountId,
                Name = "Origem",
                IsActive = true
            });

        accountApiClient
            .Setup(x => x.ResolveAccountByNumberAsync("2222222222", "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = sourceAccountId,
                Name = "Destino",
                IsActive = true
            });

        var handler = new PerformTransferCommandHandler(
            Mock.Of<ITransferRepository>(),
            idempotency.Object,
            currentUser.Object,
            accountApiClient.Object,
            Mock.Of<ITransferEventPublisher>());

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_TRANSFER");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Debit_Fails()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();

        var idempotency = new Mock<ITransferIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("transfer", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(sourceAccountId);
        currentUser.Setup(x => x.BearerToken).Returns("TOKEN");

        var accountApiClient = new Mock<IAccountApiClient>();
        accountApiClient
            .Setup(x => x.GetCurrentAccountAsync("TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = sourceAccountId,
                Name = "Origem",
                IsActive = true
            });

        accountApiClient
            .Setup(x => x.ResolveAccountByNumberAsync("2222222222", "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = destinationAccountId,
                Name = "Destino",
                IsActive = true
            });

        accountApiClient
            .Setup(x => x.CreateDebitAsync("req-1-debit", 100m, "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AccountMovementResult.Failure("TRANSFER_DEBIT_FAILED", "Falha no débito."));

        var handler = new PerformTransferCommandHandler(
            Mock.Of<ITransferRepository>(),
            idempotency.Object,
            currentUser.Object,
            accountApiClient.Object,
            Mock.Of<ITransferEventPublisher>());

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TRANSFER_DEBIT_FAILED");
    }

    [Fact]
    public async Task Handle_Should_Rollback_When_Credit_Fails()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();

        var idempotency = new Mock<ITransferIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("transfer", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(sourceAccountId);
        currentUser.Setup(x => x.BearerToken).Returns("TOKEN");

        var accountApiClient = new Mock<IAccountApiClient>();
        accountApiClient
            .Setup(x => x.GetCurrentAccountAsync("TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = sourceAccountId,
                Name = "Origem",
                IsActive = true
            });

        accountApiClient
            .Setup(x => x.ResolveAccountByNumberAsync("2222222222", "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountLookupResult
            {
                AccountId = destinationAccountId,
                Name = "Destino",
                IsActive = true
            });

        accountApiClient
            .Setup(x => x.CreateDebitAsync("req-1-debit", 100m, "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AccountMovementResult.Success());

        accountApiClient
            .Setup(x => x.CreateCreditByAccountIdAsync("req-1-credit", destinationAccountId, 100m, "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AccountMovementResult.Failure("TRANSFER_CREDIT_FAILED", "Falha no crédito."));

        accountApiClient
            .Setup(x => x.RevertDebitWithCreditAsync("req-1-rollback", 100m, "TOKEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(AccountMovementResult.Success());

        var handler = new PerformTransferCommandHandler(
            Mock.Of<ITransferRepository>(),
            idempotency.Object,
            currentUser.Object,
            accountApiClient.Object,
            Mock.Of<ITransferEventPublisher>());

        // Act
        var result = await handler.Handle(
            new PerformTransferCommand("req-1", "2222222222", 100m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TRANSFER_CREDIT_FAILED");

        accountApiClient.Verify(
            x => x.RevertDebitWithCreditAsync("req-1-rollback", 100m, "TOKEN", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}