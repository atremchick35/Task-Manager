using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskManager.Infrastructure.Persistence.Configurations;

internal sealed class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.ToTable("revoked_tokens");
        builder.HasKey(t => t.TokenId);
        builder.Property(t => t.TokenId).HasMaxLength(RevokedToken.TokenIdMaxLength);
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.HasIndex(t => t.ExpiresAt);
    }
}
