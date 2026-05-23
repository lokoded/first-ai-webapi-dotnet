using System.Text.RegularExpressions;

namespace FirstWebApi.Domain.ValueObjects;

public readonly partial record struct Email
{
    public string Endereco { get; }

    public Email(string endereco)
    {
        ArgumentNullException.ThrowIfNull(endereco);

        if (!EmailRegex().IsMatch(endereco))
            throw new ArgumentException("Email inválido.");

        Endereco = endereco.ToLowerInvariant();
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public override string ToString() => Endereco ?? throw new InvalidOperationException("Email não inicializado.");
}
