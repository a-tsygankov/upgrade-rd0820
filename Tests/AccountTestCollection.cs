using Xunit;

namespace Tests
{
    /// <summary>
    /// Test collection fixture
    /// </summary>
    [CollectionDefinition("Account Tests")]
    public class AccountTestCollection : ICollectionFixture<AccountFixture>
    { }
}
