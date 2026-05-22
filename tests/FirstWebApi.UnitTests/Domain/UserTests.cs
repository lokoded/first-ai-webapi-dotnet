using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.ValueObjects;
using FluentAssertions;

namespace FirstWebApi.UnitTests.Domain;

public class UserTests
{
    [Theory]
    [InlineData("123.456.789-09")]
    [InlineData("12345678909")]
    [InlineData("529.982.247-25")]
    public void CreateCpf_WithValidNumber_ShouldReturnSuccess(string cpfInput)
    {
        var cpf = new Cpf(cpfInput);
        cpf.Numero.Should().HaveLength(11);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("111.111.111-11")]
    public void CreateCpf_WithInvalidNumber_ShouldThrowException(string cpfInput)
    {
        Action act = () => new Cpf(cpfInput);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateCpf_WithNullValue_ShouldThrowException()
    {
        Action act = () => new Cpf(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TwoCpfsWithSameNumber_ShouldBeEqual()
    {
        var cpf1 = new Cpf("529.982.247-25");
        var cpf2 = new Cpf("52998224725");

        cpf1.Should().Be(cpf2);
        (cpf1 == cpf2).Should().BeTrue();
    }

    [Fact]
    public void FormattedCpf_ShouldReturnWithMask()
    {
        var cpf = new Cpf("52998224725");
        cpf.Formatado().Should().Be("529.982.247-25");
    }

    [Fact]
    public void CreateEmail_WithValidAddress_ShouldReturnSuccess()
    {
        var email = new Email("teste@example.com");
        email.Endereco.Should().Be("teste@example.com");
    }

    [Fact]
    public void CreateEmail_WithInvalidAddress_ShouldThrowException()
    {
        Action act = () => new Email("invalido");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateEmail_WithNullValue_ShouldThrowException()
    {
        Action act = () => new Email(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Email_ShouldBeConvertedToLowerCase()
    {
        var email = new Email("Teste@Example.COM");
        email.Endereco.Should().Be("teste@example.com");
    }

    [Fact]
    public void CreateUser_ShouldHaveValidId()
    {
        var user = new User("Joao", "joao", "joao@email.com");

        user.Id.Should().NotBeEmpty();
        user.Nome.Should().Be("Joao");
        user.Email.Should().Be("joao@email.com");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
