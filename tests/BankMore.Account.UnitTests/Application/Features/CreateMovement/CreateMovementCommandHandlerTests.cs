using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Application.Features.CreateMovement;
using BankMore.Account.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BankMore.Account.UnitTests.Application.Features.CreateMovement;

public sealed class CreateMovementCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Movement_For_Logged_Account()
    {
        // Arrange
        var account = CurrentAccount.Create("1234567890", "Edio Rhoden", "52998224725", "HASH");

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var movementRepository = new Mock<IMovementRepository>();

        var idempotency = new Mock<IIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("movement", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(account.Id);

        var handler = new CreateMovementCommandHandler(
            accountRepository.Object,
            movementRepository.Object,
            idempotency.Object,
            currentUser.Object);

        // Act
        var result = await handler.Handle(
            new CreateMovementCommand("req-1", null, null, 100m, "C"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        movementRepository.Verify(
            x => x.AddAsync(It.IsAny<Movement>(), It.IsAny<CancellationToken>()),
            Times.Once);

        idempotency.Verify(
            x => x.RegisterAsync("movement", "req-1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Request_Is_Duplicate()
    {
        // Arrange
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(Guid.NewGuid());

        var idempotency = new Mock<IIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("movement", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateMovementCommandHandler(
            Mock.Of<IAccountRepository>(),
            Mock.Of<IMovementRepository>(),
            idempotency.Object,
            currentUser.Object);

        // Act
        var result = await handler.Handle(
            new CreateMovementCommand("req-1", null, null, 100m, "C"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Value_Is_Invalid()
    {
        // Arrange
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(Guid.NewGuid());

        var handler = new CreateMovementCommandHandler(
            Mock.Of<IAccountRepository>(),
            Mock.Of<IMovementRepository>(),
            Mock.Of<IIdempotencyService>(),
            currentUser.Object);

        // Act
        var result = await handler.Handle(
            new CreateMovementCommand("req-1", null, null, 0m, "C"),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_VALUE");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Type_Is_Invalid()
    {
        // Arrange
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(Guid.NewGuid());

        var handler = new CreateMovementCommandHandler(
            Mock.Of<IAccountRepository>(),
            Mock.Of<IMovementRepository>(),
            Mock.Of<IIdempotencyService>(),
            currentUser.Object);

        // Act
        var result = await handler.Handle(
            new CreateMovementCommand("req-1", null, null, 100m, "X"),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_TYPE");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Debit_Is_Requested_For_Different_Account()
    {
        // Arrange
        var loggedAccount = CurrentAccount.Create("1111111111", "Titular", "52998224725", "HASH");
        var targetAccount = CurrentAccount.Create("2222222222", "Destino", "39053344705", "HASH");

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByIdAsync(loggedAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loggedAccount);
        accountRepository
            .Setup(x => x.GetByAccountNumberAsync("2222222222", It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetAccount);

        var idempotency = new Mock<IIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("movement", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(loggedAccount.Id);

        var handler = new CreateMovementCommandHandler(
            accountRepository.Object,
            Mock.Of<IMovementRepository>(),
            idempotency.Object,
            currentUser.Object);

        // Act
        var result = await handler.Handle(
            new CreateMovementCommand("req-1", "2222222222", null, 50m, "D"),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_TYPE");
    }

    [Fact]
    public async Task Handle_Should_Allow_Credit_When_Target_Account_Is_Different_From_Logged_Account()
    {
        // Arrange
        var loggedAccount = CurrentAccount.Create("1111111111", "Titular", "52998224725", "HASH");
        var targetAccount = CurrentAccount.Create("2222222222", "Destino", "39053344705", "HASH");

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByIdAsync(loggedAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loggedAccount);
        accountRepository
            .Setup(x => x.GetByAccountNumberAsync("2222222222", It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetAccount);

        var movementRepository = new Mock<IMovementRepository>();

        var idempotency = new Mock<IIdempotencyService>();
        idempotency
            .Setup(x => x.ExistsAsync("movement", "req-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(loggedAccount.Id);

        var handler = new CreateMovementCommandHandler(
            accountRepository.Object,
            movementRepository.Object,
            idempotency.Object,
            currentUser.Object);

        // Act
        var result = await handler.Handle(
            new CreateMovementCommand("req-1", "2222222222", null, 50m, "C"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        movementRepository.Verify(
            x => x.AddAsync(It.IsAny<Movement>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}