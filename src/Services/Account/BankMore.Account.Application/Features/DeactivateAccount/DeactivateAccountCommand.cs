using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.DeactivateAccount;

public sealed record DeactivateAccountCommand(string Password) : ICommand;