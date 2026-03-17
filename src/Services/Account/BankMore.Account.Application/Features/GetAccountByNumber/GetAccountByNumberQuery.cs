using BankMore.Account.Application.DTOs;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.GetAccountByNumber;

public sealed record GetAccountByNumberQuery(string AccountNumber) : IQuery<AccountSummaryResponse>;