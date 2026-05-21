using System.Text.RegularExpressions;

namespace FirstWebApi.Domain.ValueObjects;

public partial class Email
{
    public string Endereco { get; }

    protected Email() : this("placeholder@email.com") { }

    public Email(string endereco)
    {
        ArgumentNullException.ThrowIfNull(endereco);

        if (!EmailRegex().IsMatch(endereco))
            throw new ArgumentException("Email inválido.");

        Endereco = endereco.ToLowerInvariant();
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public override string ToString() => Endereco;
    public override bool Equals(object? obj) => obj is Email other && Endereco == other.Endereco;
    public override int GetHashCode() => Endereco.GetHashCode();
}

