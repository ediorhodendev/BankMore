using BankMore.BuildingBlocks.Application.Common;
using MediatR;

namespace BankMore.BuildingBlocks.Application.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}