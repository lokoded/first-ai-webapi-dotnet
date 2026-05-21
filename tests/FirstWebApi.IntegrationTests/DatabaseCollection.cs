using Microsoft.AspNetCore.Mvc.Testing;

namespace FirstWebApi.IntegrationTests;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<FirstWebApiFactory>
{
}
