using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Application.Features.Login;
using BankMore.Account.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BankMore.Account.UnitTests.Application.Features.Login;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Login_With_Cpf()
    {
        var account = CurrentAccount.Create("1234567890", "Edio Rhoden", "52998224725", "HASH");

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByCpfAsync("52998224725", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var passwordHasher = new Mock<IPasswordHasherService>();
        passwordHasher.Setup(x => x.Verify("123456", "HASH")).Returns(true);

        var tokenProvider = new Mock<ITokenProvider>();
        tokenProvider.Setup(x => x.GenerateToken(account.Id, account.AccountNumber, account.Name))
            .Returns("TOKEN");

        var handler = new LoginCommandHandler(
            accountRepository.Object,
            passwordHasher.Object,
            tokenProvider.Object);

        var result = await handler.Handle(new LoginCommand("52998224725", "123456"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("TOKEN");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Password_Is_Invalid()
    {
        var account = CurrentAccount.Create("1234567890", "Edio Rhoden", "52998224725", "HASH");

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByCpfAsync("52998224725", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var passwordHasher = new Mock<IPasswordHasherService>();
        passwordHasher.Setup(x => x.Verify("123456", "HASH")).Returns(false);

        var handler = new LoginCommandHandler(
            accountRepository.Object,
            passwordHasher.Object,
            Mock.Of<ITokenProvider>());

        var result = await handler.Handle(new LoginCommand("52998224725", "123456"), CancellationToken.None);

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
            .Setup(x => x.GetByCpfAsync("52998224725", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var passwordHasher = new Mock<IPasswordHasherService>();
        passwordHasher.Setup(x => x.Verify("123456", "HASH")).Returns(true);

        var handler = new LoginCommandHandler(
            accountRepository.Object,
            passwordHasher.Object,
            Mock.Of<ITokenProvider>());

        var result = await handler.Handle(new LoginCommand("52998224725", "123456"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INACTIVE_ACCOUNT");
    }
}