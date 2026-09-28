using TaskManager.Infrastructure.Security;

namespace TaskManager.Tests.Unit;

public sealed class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Verify_AcceptsOriginalPassword()
    {
        var hash = _hasher.Hash("correct horse battery staple");

        Assert.True(_hasher.Verify("correct horse battery staple", hash));
        Assert.False(_hasher.Verify("wrong password", hash));
    }

    [Fact]
    public void Hash_UsesRandomSalt()
    {
        Assert.NotEqual(_hasher.Hash("password123"), _hasher.Hash("password123"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("garbage")]
    [InlineData("v1.100.not-base64.not-base64")]
    public void Verify_RejectsMalformedHash(string hash)
    {
        Assert.False(_hasher.Verify("password123", hash));
    }
}
