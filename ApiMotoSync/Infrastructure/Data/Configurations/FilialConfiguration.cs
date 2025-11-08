using ApiMotoSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiMotoSync.Infrastructure.Data.Configurations;

public class FilialConfiguration : IEntityTypeConfiguration<Filial>
{
    public void Configure(EntityTypeBuilder<Filial> builder)
    {
        builder.ToTable("FILIAIS");

        builder.HasKey(filial => filial.Id);

        builder.Property(filial => filial.Nome)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(filial => filial.Codigo)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(filial => filial.Endereco)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(filial => filial.Cidade)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(filial => filial.Estado)
            .HasMaxLength(60)
            .IsRequired();

        builder.HasIndex(filial => filial.Codigo)
            .IsUnique();

        builder.HasMany(filial => filial.Motos)
            .WithOne(moto => moto.Filial)
            .HasForeignKey(moto => moto.FilialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(filial => filial.Usuarios)
            .WithOne(usuario => usuario.Filial)
            .HasForeignKey(usuario => usuario.FilialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

