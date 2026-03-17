using BankMore.BuildingBlocks.Application.Common;

namespace BankMore.Transfer.Domain.Common;

public static class TransferErrors
{
    public static readonly Error InvalidAccount =
        new("INVALID_ACCOUNT", "A conta informada não existe.");

    public static readonly Error InactiveAccount =
        new("INACTIVE_ACCOUNT", "A conta informada está inativa.");

    public static readonly Error InvalidValue =
        new("INVALID_VALUE", "O valor informado deve ser maior que zero.");

    public static readonly Error Unauthorized =
        new("USER_UNAUTHORIZED", "Usuário não autorizado.");

    public static readonly Error DuplicateRequest =
        new("DUPLICATE_REQUEST", "A requisição já foi processada anteriormente.");

    public static readonly Error InvalidTransfer =
        new("INVALID_TRANSFER", "A transferência informada é inválida.");
}