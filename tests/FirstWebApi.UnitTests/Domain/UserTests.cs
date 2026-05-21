using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Enums;
using FirstWebApi.Domain.ValueObjects;
using FluentAssertions;

namespace FirstWebApi.UnitTests.Domain;

public class UserTests
{
    [Theory]
    [InlineData("123.456.789-09")]
    [InlineData("12345678909")]
    [InlineData("529.982.247-25")]
    public void CriarCpf_ComNumeroValido_DeveRetornarSucesso(string cpfInput)
    {
        var cpf = new Cpf(cpfInput);
        cpf.Numero.Should().HaveLength(11);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("111.111.111-11")]
    public void CriarCpf_ComNumeroInvalido_DeveLancarExcecao(string cpfInput)
    {
        Action act = () => new Cpf(cpfInput);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CriarCpf_ComValorNulo_DeveLancarExcecao()
    {
        Action act = () => new Cpf(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DoisCpfsComMesmoNumero_DevemSerIguais()
    {
        var cpf1 = new Cpf("529.982.247-25");
        var cpf2 = new Cpf("52998224725");

        cpf1.Should().Be(cpf2);
        (cpf1 == cpf2).Should().BeTrue();
    }

    [Fact]
    public void CpfFormatado_DeveRetornarComMascara()
    {
        var cpf = new Cpf("52998224725");
        cpf.Formatado().Should().Be("529.982.247-25");
    }

    [Fact]
    public void CriarEmail_ComEnderecoValido_DeveRetornarSucesso()
    {
        var email = new Email("teste@example.com");
        email.Endereco.Should().Be("teste@example.com");
    }

    [Fact]
    public void CriarEmail_ComEnderecoInvalido_DeveLancarExcecao()
    {
        Action act = () => new Email("invalido");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CriarEmail_ComValorNulo_DeveLancarExcecao()
    {
        Action act = () => new Email(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Email_DeveSerConvertidoParaMinusculo()
    {
        var email = new Email("Teste@Example.COM");
        email.Endereco.Should().Be("teste@example.com");
    }

    [Fact]
    public void CriarUsuario_DeveTerIdEUserRole()
    {
        var user = new User("Joao", "joao", "joao@email.com");

        user.Id.Should().NotBeEmpty();
        user.Nome.Should().Be("Joao");
        user.Email.Should().Be("joao@email.com");
        user.Role.Should().Be(EUserRole.User);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
