using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Application.Features.DeactivateAccount;
using BankMore.Account.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BankMore.Account.UnitTests.Application.Features.DeactivateAccount;

public sealed class DeactivateAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Deactivate_Account()
    {
        var account = CurrentAccount.Create("1234567890", "Edio Rhoden", "52998224725", "HASH");

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var passwordHasher = new Mock<IPasswordHasherService>();
        passwordHasher.Setup(x => x.Verify("123456", "HASH")).Returns(true);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(account.Id);

        var handler = new DeactivateAccountCommandHandler(
            accountRepository.Object,
            passwordHasher.Object,
            currentUser.Object);

        var result = await handler.Handle(new DeactivateAccountCommand("123456"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        account.IsActive.Should().BeFalse();

        accountRepository.Verify(
            x => x.UpdateAsync(account, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Not_Authenticated()
    {
        var handler = new DeactivateAccountCommandHandler(
            Mock.Of<IAccountRepository>(),
            Mock.Of<IPasswordHasherService>(),
            Mock.Of<ICurrentUserService>());

        var result = await handler.Handle(new DeactivateAccountCommand("123456"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("USER_UNAUTHORIZED");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Password_Is_Invalid()
    {
        var account = CurrentAccount.Create("1234567890", "Edio Rhoden", "52998224725", "HASH");

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var passwordHasher = new Mock<IPasswordHasherService>();
        passwordHasher.Setup(x => x.Verify("123456", "HASH")).Returns(false);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.AccountId).Returns(account.Id);

        var handler = new DeactivateAccountCommandHandler(
            accountRepository.Object,
            passwordHasher.Object,
            currentUser.Object);

        var result = await handler.Handle(new DeactivateAccountCommand("123456"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_PASSWORD");
    }
}