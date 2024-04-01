using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MpSo.Domain.Entities;

namespace MpSo.Entities;

public class Tag : BaseEntity
{
    public int Count { get; set; }
    public double PercentageShare { get; set; }
    public bool HasSynonyms { get; set; }
    public bool IsRequired { get; set; }
    public bool IsModeratorOnly { get; set; }
    public required string Name { get; set; }

    internal class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("tags");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(t => t.Count)
                .HasColumnName("count");

            builder.Property(t => t.PercentageShare)
                .HasColumnName("percentage_share");

            builder.Property(t => t.HasSynonyms)
                .HasColumnName("has_synonyms");

            builder.Property(t => t.IsRequired)
                .HasColumnName("is_required");

            builder.Property(t => t.IsModeratorOnly)
                .HasColumnName("is_moderator_only");

            builder.Property(t => t.Name)
                .HasColumnName("name")
                .HasMaxLength(50);
        }
    }
}
