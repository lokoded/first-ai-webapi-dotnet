namespace FirstWebApi.Domain.ValueObjects;

public record class Cpf
{
    public string Numero { get; }

    public Cpf(string numero)
    {
        ArgumentNullException.ThrowIfNull(numero);

        var apenasDigitos = new string([.. numero.Where(char.IsDigit)]);

        if (apenasDigitos.Length != 11)
            throw new ArgumentException("CPF deve conter exatamente 11 dígitos.");

        if (apenasDigitos.All(c => c == apenasDigitos[0]))
            throw new ArgumentException("CPF inválido.");

        if (!ValidarDigitos(apenasDigitos))
            throw new ArgumentException("CPF inválido.");

        Numero = apenasDigitos;
    }

    private static bool ValidarDigitos(string cpf)
    {
        var digitos = cpf.Select(c => c - '0').ToArray();

        var digito1 = CalcularDigito(digitos, 9);
        if (digitos[9] != digito1) return false;

        var digito2 = CalcularDigito(digitos, 10);
        return digitos[10] == digito2;
    }

    private static int CalcularDigito(int[] digitos, int posicao)
    {
        var soma = 0;
        for (int i = 0; i < posicao; i++)
            soma += digitos[i] * (posicao + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override string ToString() => Numero ?? throw new InvalidOperationException("CPF não inicializado.");
    public string Formatado()
    {
        if (Numero is null)
            throw new InvalidOperationException("CPF não inicializado.");
        return $"{Numero[..3]}.{Numero[3..6]}.{Numero[6..9]}-{Numero[9..]}";
    }
}
