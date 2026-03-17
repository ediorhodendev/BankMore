using BankMore.BuildingBlocks.Application.Common;

namespace BankMore.Account.Domain.Common;

public static class AccountErrors
{
    public static readonly Error InvalidDocument =
        new("INVALID_DOCUMENT", "O CPF informado é inválido.");

    public static readonly Error InvalidAccount =
        new("INVALID_ACCOUNT", "A conta corrente informada não existe.");

    public static readonly Error InactiveAccount =
        new("INACTIVE_ACCOUNT", "A conta corrente está inativa.");

    public static readonly Error InvalidValue =
        new("INVALID_VALUE", "O valor informado deve ser maior que zero.");

    public static readonly Error InvalidType =
        new("INVALID_TYPE", "O tipo de movimentação informado é inválido.");

    public static readonly Error Unauthorized =
        new("USER_UNAUTHORIZED", "Usuário ou senha inválidos.");

    public static readonly Error InvalidPassword =
        new("INVALID_PASSWORD", "A senha informada é inválida.");

    public static readonly Error DuplicateRequest =
        new("DUPLICATE_REQUEST", "A requisição já foi processada anteriormente.");
}