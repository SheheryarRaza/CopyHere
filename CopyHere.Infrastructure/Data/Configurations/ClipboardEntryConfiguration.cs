using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CopyHere.Infrastructure.Data.Configurations
{
    public class ClipboardEntryConfiguration : IEntityTypeConfiguration<ClipboardEntry>
    {
        public void Configure(EntityTypeBuilder<ClipboardEntry> builder)
        {
            builder.HasKey(ce => ce.Id);
            builder.Property(ce => ce.ContentType).IsRequired().HasConversion<string>();
            builder.Property(ce => ce.ContentText).HasMaxLength(4000);
            builder.Property(ce => ce.ContentBytes).HasColumnType("varbinary(max)");
            builder.Property(ce => ce.CreatedAt).IsRequired();

            // New properties configuration
            builder.Property(ce => ce.IsPinned).IsRequired().HasDefaultValue(false);
            builder.Property(ce => ce.IsArchived).IsRequired().HasDefaultValue(false);
            builder.Property(ce => ce.Tags).HasMaxLength(500);
        }
    }
}
