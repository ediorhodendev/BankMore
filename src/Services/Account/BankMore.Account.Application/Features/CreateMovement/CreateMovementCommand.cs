using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.CreateMovement;

public sealed record CreateMovementCommand(
    string RequestId,
    string? AccountNumber,
    Guid? TargetAccountId,
    decimal Amount,
    string Type) : ICommand;