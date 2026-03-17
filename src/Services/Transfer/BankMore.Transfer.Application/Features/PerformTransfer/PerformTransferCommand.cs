using BankMore.BuildingBlocks.Application.Messaging;
using BankMore.Transfer.Application.DTOs;

namespace BankMore.Transfer.Application.Features.PerformTransfer;

public sealed record PerformTransferCommand(
    string RequestId,
    string DestinationAccountNumber,
    decimal Amount) : ICommand<TransferResponse>;