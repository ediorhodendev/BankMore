using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Application.Abstractions.Services;
using BankMore.Account.Application.Features.CreateAccount;
using BankMore.Account.Domain.Entities;
using FluentAssertions;
using Moq;

namespace BankMore.Account.UnitTests.Application.Features.CreateAccount;

public sealed class CreateAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Account_When_Request_Is_Valid()
    {
        var accountRepository = new Mock<IAccountRepository>();
        var passwordHasher = new Mock<IPasswordHasherService>();
        var numberGenerator = new Mock<IAccountNumberGenerator>();

        numberGenerator.Setup(x => x.Generate()).Returns("1234567890");
        passwordHasher.Setup(x => x.Hash("123456")).Returns("HASH");
        accountRepository
            .Setup(x => x.GetByCpfAsync("52998224725", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrentAccount?)null);

        var handler = new CreateAccountCommandHandler(
            accountRepository.Object,
            passwordHasher.Object,
            numberGenerator.Object);

        var command = new CreateAccountCommand("Edio Rhoden", "52998224725", "123456");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccountNumber.Should().Be("1234567890");

        accountRepository.Verify(
            x => x.AddAsync(It.IsAny<CurrentAccount>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Cpf_Is_Invalid()
    {
        var handler = new CreateAccountCommandHandler(
            Mock.Of<IAccountRepository>(),
            Mock.Of<IPasswordHasherService>(),
            Mock.Of<IAccountNumberGenerator>());

        var command = new CreateAccountCommand("Edio Rhoden", "123", "123456");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_DOCUMENT");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Cpf_Already_Exists()
    {
        var existingAccount = CurrentAccount.Create(
            "1234567890",
            "Edio Rhoden",
            "52998224725",
            "HASH");

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(x => x.GetByCpfAsync("52998224725", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        var handler = new CreateAccountCommandHandler(
            accountRepository.Object,
            Mock.Of<IPasswordHasherService>(),
            Mock.Of<IAccountNumberGenerator>());

        var command = new CreateAccountCommand("Edio Rhoden", "52998224725", "123456");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ACCOUNT_ALREADY_EXISTS");
    }
}