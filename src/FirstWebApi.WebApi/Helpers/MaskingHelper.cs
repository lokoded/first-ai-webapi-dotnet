using FirstWebApi.Application.DTOs;
using FirstWebApi.Application.DTOs.Response;

namespace FirstWebApi.WebApi.Helpers;

public static class MaskingHelper
{
    public static string? MaskCpf(string? cpf)
    {
        if (string.IsNullOrEmpty(cpf) || cpf.Length < 11)
            return cpf;

        return $"***.{cpf[3..6]}.{cpf[6..9]}-**";
    }

    public static string? MaskRg(string? rg)
    {
        if (string.IsNullOrEmpty(rg) || rg.Length < 4)
            return rg;

        return $"*****-{rg[^4..]}";
    }

    public static EnderecoInfo MaskEndereco(EnderecoInfo endereco) => new()
    {
        Cidade = endereco.Cidade,
        Estado = endereco.Estado
    };

    public static void ApplyMask(UserResponse response)
    {
        response.Cpf = MaskCpf(response.Cpf);
        response.Rg = MaskRg(response.Rg);
        response.Endereco = response.Endereco is not null ? MaskEndereco(response.Endereco) : null;
        response.HasFullData = false;
    }
}
