using BankMore.BuildingBlocks.Application.Common;
using MediatR;

namespace BankMore.BuildingBlocks.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}