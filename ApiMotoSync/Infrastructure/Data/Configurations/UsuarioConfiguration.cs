using ApiMotoSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiMotoSync.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("USUARIOS");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Nome)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(usuario => usuario.Email)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(usuario => usuario.Cargo)
            .HasMaxLength(80)
            .IsRequired();

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique();

        builder.HasOne(usuario => usuario.Filial)
            .WithMany(filial => filial.Usuarios)
            .HasForeignKey(usuario => usuario.FilialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(usuario => usuario.MotosGerenciadas)
            .WithOne(moto => moto.Gestor)
            .HasForeignKey(moto => moto.GestorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

