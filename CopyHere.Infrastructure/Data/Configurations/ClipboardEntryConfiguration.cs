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

            builder.Property(ce => ce.ContentType)
                .IsRequired()
                .HasConversion<string>(); // Store enum as string in DB

            builder.Property(ce => ce.ContentText)
                .HasMaxLength(4000); // Max length for text content, or NVARCHAR(MAX) if truly large

            builder.Property(ce => ce.ContentBytes)
                .HasColumnType("varbinary(max)"); // Store binary data

            builder.Property(ce => ce.CreatedAt)
                .IsRequired();
        }
    }
}
