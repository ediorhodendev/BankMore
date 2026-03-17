using BankMore.BuildingBlocks.Application.Messaging;
using BankMore.Fee.Application.DTOs;

namespace BankMore.Fee.Application.Features.ProcessFee;

public sealed record ProcessFeeCommand(
    Guid TransferId,
    Guid AccountId,
    decimal Amount) : ICommand<FeeResponse>;