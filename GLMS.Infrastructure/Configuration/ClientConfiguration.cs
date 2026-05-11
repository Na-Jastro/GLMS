using GLMS.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GLMS.Infrastructure.Configuration
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.ContactDetails)
                   .HasMaxLength(250);

            builder.Property(c => c.Region)
                   .HasMaxLength(100);

            builder.HasMany(c => c.Contracts)
                   .WithOne(c => c.Client)
                   .HasForeignKey(c => c.ClientId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
