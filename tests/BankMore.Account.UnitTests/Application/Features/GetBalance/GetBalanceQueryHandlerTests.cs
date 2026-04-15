using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Application.Features.GetBalance;
using BankMore.Account.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BankMore.Account.UnitTests.Application.Features.GetBalance;

public sealed class GetBalanceQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Balance()
    {
        // Arrange
        var account = CurrentAccount.Create("1234567890", "Edio Rhoden", "52998224725", "HASH");

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var movementRepository = new Mock<IMovementRepository>();
        movementRepository
            .Setup(x => x.GetBalanceAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(150.75m);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(account.Id);

        var handler = new GetBalanceQueryHandler(
            accountRepository.Object,
            movementRepository.Object,
            currentUser.Object);

        // Act
        var result = await handler.Handle(new GetBalanceQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Balance.Should().Be(150.75m);
        result.Value.AccountNumber.Should().Be(account.AccountNumber);
        result.Value.Name.Should().Be(account.Name);
    }

    [Fact]
    public async Task Handle_Should_Return_Zero_When_There_Are_No_Movements()
    {
        // Arrange
        var account = CurrentAccount.Create("1234567890", "Edio Rhoden", "52998224725", "HASH");

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var movementRepository = new Mock<IMovementRepository>();
        movementRepository
            .Setup(x => x.GetBalanceAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(account.Id);

        var handler = new GetBalanceQueryHandler(
            accountRepository.Object,
            movementRepository.Object,
            currentUser.Object);

        // Act
        var result = await handler.Handle(new GetBalanceQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Not_Authenticated()
    {
        // Arrange
        var handler = new GetBalanceQueryHandler(
            Mock.Of<IAccountRepository>(),
            Mock.Of<IMovementRepository>(),
            Mock.Of<ICurrentUserService>());

        // Act
        var result = await handler.Handle(new GetBalanceQuery(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("USER_UNAUTHORIZED");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Account_Does_Not_Exist()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrentAccount?)null);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(accountId);

        var handler = new GetBalanceQueryHandler(
            accountRepository.Object,
            Mock.Of<IMovementRepository>(),
            currentUser.Object);

        // Act
        var result = await handler.Handle(new GetBalanceQuery(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_ACCOUNT");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Account_Is_Inactive()
    {
        // Arrange
        var account = CurrentAccount.Create("1234567890", "Edio Rhoden", "52998224725", "HASH");
        account.Deactivate();

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(account.Id);

        var handler = new GetBalanceQueryHandler(
            accountRepository.Object,
            Mock.Of<IMovementRepository>(),
            currentUser.Object);

        // Act
        var result = await handler.Handle(new GetBalanceQuery(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INACTIVE_ACCOUNT");
    }
}