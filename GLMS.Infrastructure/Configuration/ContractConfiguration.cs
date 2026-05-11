using GLMS.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GLMS.Infrastructure.Configuration
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.ToTable("Contracts");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.StartDate)
                   .IsRequired();

            builder.Property(c => c.EndDate)
                   .IsRequired();

            builder.Property(c => c.Status)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(c => c.ServiceLevel)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.SignedAgreementPath)
                   .HasMaxLength(500);

            builder.HasOne(c => c.Client)
                   .WithMany(c => c.Contracts)
                   .HasForeignKey(c => c.ClientId);

            builder.HasMany(c => c.ServiceRequests)
                   .WithOne(sr => sr.Contract)
                   .HasForeignKey(sr => sr.ContractId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
