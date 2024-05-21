using System;
using System.Collections.Generic;
using ConversorDadosXPTO.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ConversorDadosXPTO.Context;

public partial class DadosXptoContext : DbContext
{
    public DadosXptoContext()
    {
    }

    public DadosXptoContext(DbContextOptions<DadosXptoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cidadao> Cidadaos { get; set; }

    public virtual DbSet<Dado> Dados { get; set; }

    public virtual DbSet<ProgramaSocial> ProgramaSocials { get; set; }

    public virtual DbSet<UfCidade> UfCidades { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=dados_xpto;user=admin;password=admin", ServerVersion.Parse("8.4.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_general_ci")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<Cidadao>(entity =>
        {
            entity.HasKey(e => e.Idcidadao).HasName("PRIMARY");

            entity.ToTable("cidadao");

            entity.Property(e => e.Idcidadao).HasColumnName("idcidadao");
            entity.Property(e => e.Cpf)
                .HasMaxLength(11)
                .HasColumnName("cpf");
            entity.Property(e => e.Nome)
                .HasMaxLength(45)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<Dado>(entity =>
        {
            entity.HasKey(e => e.Iddados).HasName("PRIMARY");

            entity.ToTable("dados");

            entity.HasIndex(e => e.Idcidadao, "fk_dados_cidadao1_idx");

            entity.HasIndex(e => e.IdprogramaSocial, "fk_dados_programa_social1_idx");

            entity.HasIndex(e => e.IdufCidade, "fk_dados_uf_cidade_idx");

            entity.Property(e => e.Iddados).HasColumnName("iddados");
            entity.Property(e => e.Idcidadao).HasColumnName("idcidadao");
            entity.Property(e => e.IdprogramaSocial).HasColumnName("idprograma_social");
            entity.Property(e => e.IdufCidade).HasColumnName("iduf_cidade");
            entity.Property(e => e.MesAno)
                .HasMaxLength(6)
                .HasColumnName("mes_ano");
            entity.Property(e => e.Valor).HasColumnName("valor");

            entity.HasOne(d => d.IdcidadaoNavigation).WithMany(p => p.Dados)
                .HasForeignKey(d => d.Idcidadao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dados_cidadao1");

            entity.HasOne(d => d.IdprogramaSocialNavigation).WithMany(p => p.Dados)
                .HasForeignKey(d => d.IdprogramaSocial)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dados_programa_social1");

            entity.HasOne(d => d.IdufCidadeNavigation).WithMany(p => p.Dados)
                .HasForeignKey(d => d.IdufCidade)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dados_uf_cidade");
        });

        modelBuilder.Entity<ProgramaSocial>(entity =>
        {
            entity.HasKey(e => e.IdprogramaSocial).HasName("PRIMARY");

            entity.ToTable("programa_social");

            entity.Property(e => e.IdprogramaSocial).HasColumnName("idprograma_social");
            entity.Property(e => e.Nome)
                .HasMaxLength(45)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<UfCidade>(entity =>
        {
            entity.HasKey(e => e.IdufCidade).HasName("PRIMARY");

            entity.ToTable("uf_cidade");

            entity.Property(e => e.IdufCidade).HasColumnName("iduf_cidade");
            entity.Property(e => e.Cidade)
                .HasMaxLength(45)
                .HasColumnName("cidade");
            entity.Property(e => e.Uf)
                .HasMaxLength(2)
                .HasColumnName("uf");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
