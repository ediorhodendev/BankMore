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

        var result = await handler.Handle(new GetBalanceQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Balance.Should().Be(150.75m);
        result.Value.AccountNumber.Should().Be(account.AccountNumber);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Not_Authenticated()
    {
        var handler = new GetBalanceQueryHandler(
            Mock.Of<IAccountRepository>(),
            Mock.Of<IMovementRepository>(),
            Mock.Of<ICurrentUserService>());

        var result = await handler.Handle(new GetBalanceQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("USER_UNAUTHORIZED");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Account_Is_Inactive()
    {
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

        var result = await handler.Handle(new GetBalanceQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INACTIVE_ACCOUNT");
    }
}