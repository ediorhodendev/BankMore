using BankMore.Account.Application.DTOs;
using BankMore.BuildingBlocks.Application.Messaging;

namespace BankMore.Account.Application.Features.Login;

public sealed record LoginCommand(
    string Login,
    string Password) : ICommand<LoginResponse>;