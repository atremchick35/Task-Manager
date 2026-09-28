using TaskManager.Domain.Common;
using TaskManager.Domain.Users;

namespace TaskManager.Tests.Unit;

public sealed class UserTests
{
    [Fact]
    public void Register_NormalizesEmail()
    {
        var user = User.Register("Alice", "  Alice@Example.COM ", "hash", DateTimeOffset.UtcNow);

        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal("Alice", user.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("Alice <alice@example.com>")]
    public void Register_WithInvalidEmail_Throws(string email)
    {
        Assert.Throws<DomainException>(() => User.Register("Alice", email, "hash", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Register_WithoutName_Throws()
    {
        Assert.Throws<DomainException>(() => User.Register(" ", "alice@example.com", "hash", DateTimeOffset.UtcNow));
    }
}
