using BankMore.Account.Application.Abstractions.Persistence;
using BankMore.Account.Application.Abstractions.Security;
using BankMore.Account.Application.Abstractions.Services;
using BankMore.Account.Application.DTOs;
using BankMore.Account.Domain.Common;
using BankMore.Account.Domain.Entities;
using BankMore.Account.Domain.ValueObjects;
using BankMore.BuildingBlocks.Application.Common;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.CreateAccount;

public sealed class CreateAccountCommandHandler
    : ICommandHandler<CreateAccountCommand, CreateAccountResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IAccountNumberGenerator _accountNumberGenerator;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHasherService passwordHasherService,
        IAccountNumberGenerator accountNumberGenerator)
    {
        _accountRepository = accountRepository;
        _passwordHasherService = passwordHasherService;
        _accountNumberGenerator = accountNumberGenerator;
    }

    public async Task<Result<CreateAccountResponse>> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<CreateAccountResponse>.Failure(
                new Error("INVALID_NAME", "O nome do titular é obrigatório."));

        if (!Cpf.TryCreate(request.Cpf, out var cpf))
            return Result<CreateAccountResponse>.Failure(AccountErrors.InvalidDocument);

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result<CreateAccountResponse>.Failure(AccountErrors.InvalidPassword);

        var existingAccount = await _accountRepository.GetByCpfAsync(cpf!.Value, cancellationToken);
        if (existingAccount is not null)
        {
            return Result<CreateAccountResponse>.Failure(
                new Error("ACCOUNT_ALREADY_EXISTS", "Já existe uma conta cadastrada para o CPF informado."));
        }

        var accountNumber = _accountNumberGenerator.Generate();
        var passwordHash = _passwordHasherService.Hash(request.Password);

        var account = CurrentAccount.Create(
            accountNumber: accountNumber,
            name: request.Name,
            cpf: cpf.Value,
            passwordHash: passwordHash);

        await _accountRepository.AddAsync(account, cancellationToken);

        return Result<CreateAccountResponse>.Success(new CreateAccountResponse
        {
            AccountId = account.Id,
            AccountNumber = account.AccountNumber,
            Name = account.Name
        });
    }
}