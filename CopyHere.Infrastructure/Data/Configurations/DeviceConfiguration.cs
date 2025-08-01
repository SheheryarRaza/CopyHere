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
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.DeviceName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.DeviceType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.LastSeen)
                .IsRequired();
        }
    }
}
