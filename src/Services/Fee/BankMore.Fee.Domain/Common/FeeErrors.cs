using BankMore.BuildingBlocks.Application.Common;

namespace BankMore.Fee.Domain.Common;

public static class FeeErrors
{
    public static readonly Error InvalidTransfer =
        new("INVALID_TRANSFER", "A transferência informada é inválida.");

    public static readonly Error InvalidValue =
        new("INVALID_VALUE", "O valor da tarifa deve ser maior que zero.");

    public static readonly Error InvalidAccount =
        new("INVALID_ACCOUNT", "A conta informada é inválida.");
}