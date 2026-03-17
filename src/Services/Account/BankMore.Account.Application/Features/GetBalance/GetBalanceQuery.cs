using BankMore.Account.Application.DTOs;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.GetBalance;

public sealed record GetBalanceQuery() : IQuery<BalanceResponse>;