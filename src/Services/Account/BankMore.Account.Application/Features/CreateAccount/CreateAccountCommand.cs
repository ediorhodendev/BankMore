using BankMore.Account.Application.DTOs;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.CreateAccount;

public sealed record CreateAccountCommand(
    string Name,
    string Cpf,
    string Password) : ICommand<CreateAccountResponse>;