using BankMore.Account.Application.DTOs;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.GetCurrentAccount;

public sealed record GetCurrentAccountQuery() : IQuery<AccountSummaryResponse>;