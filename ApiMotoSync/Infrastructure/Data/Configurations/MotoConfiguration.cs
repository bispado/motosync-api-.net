using ApiMotoSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiMotoSync.Infrastructure.Data.Configurations;

public class MotoConfiguration : IEntityTypeConfiguration<Moto>
{
    public void Configure(EntityTypeBuilder<Moto> builder)
    {
        builder.ToTable("MOTOS");

        builder.HasKey(moto => moto.Id);

        builder.Property(moto => moto.Modelo)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(moto => moto.Placa)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(moto => moto.Status)
            .HasMaxLength(50)
            .HasDefaultValue("Disponível")
            .IsRequired();

        builder.Property(moto => moto.Ano)
            .IsRequired();

        builder.HasIndex(moto => moto.Placa)
            .IsUnique();

        builder.HasOne(moto => moto.Filial)
            .WithMany(filial => filial.Motos)
            .HasForeignKey(moto => moto.FilialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(moto => moto.Gestor)
            .WithMany(usuario => usuario.MotosGerenciadas)
            .HasForeignKey(moto => moto.GestorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

