using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AgenciaDeTours.Models;

public partial class AgenciaToursContext : DbContext
{
    public AgenciaToursContext()
    {
    }

    public AgenciaToursContext(DbContextOptions<AgenciaToursContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Destino> Destinos { get; set; }
    public virtual DbSet<Pai> Pais { get; set; }
    public virtual DbSet<Tour> Tours { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=LOCALHOST;Database=AgenciaTours;Trusted_Connection=True;TrustServerCertificate=True");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Destino>(entity =>
        {
            entity.HasKey(e => e.DestinoId).HasName("PK__Destino__4A838EF681BB0B48");

            entity.ToTable("Destino");

            entity.Property(e => e.DestinoId).HasColumnName("DestinoID");
            entity.Property(e => e.Actividades).HasMaxLength(255);
            entity.Property(e => e.AtractivoPrincipal).HasMaxLength(100);
            entity.Property(e => e.Clima).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PaisId).HasColumnName("PaisID");
            entity.Property(e => e.TipoDestino).HasMaxLength(50);

            entity.HasOne(d => d.Pais).WithMany(p => p.Destinos)
                .HasForeignKey(d => d.PaisId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Destino__PaisID__398D8EEE");
        });

        modelBuilder.Entity<Pai>(entity =>
        {
            entity.HasKey(e => e.PaisId).HasName("PK__Pais__B501E1A532094361");

            entity.Property(e => e.PaisId).HasColumnName("PaisID");
            entity.Property(e => e.CodigoIso)
                .HasMaxLength(3)  // Longitud máxima de 3 caracteres
                .IsRequired()      // Campo obligatorio
                .HasColumnName("CodigoISO");
            entity.Property(e => e.Continente).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.IdiomaOficial).HasMaxLength(50);
            entity.Property(e => e.Moneda).HasMaxLength(50);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            // Añadir índice único para CodigoIso
            entity.HasIndex(e => e.CodigoIso)
                .IsUnique()
                .HasDatabaseName("IX_Pais_CodigoIso_Unique");
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.TourId).HasName("PK__Tour__604CEA101A086580");

            entity.ToTable("Tour");

            entity.Property(e => e.TourId).HasColumnName("TourID");
            entity.Property(e => e.DestinoId).HasColumnName("DestinoID");
            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasComputedColumnSql("(case when [FechaHoraFin]>=getdate() then 'Vigente' else 'No Vigente' end)", false);
            entity.Property(e => e.FechaHoraFin).HasColumnType("datetime");
            entity.Property(e => e.Itbis)
                .HasComputedColumnSql("([Precio]*(0.18))", true)
                .HasColumnType("numeric(13, 4)")
                .HasColumnName("ITBIS");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.PaisId).HasColumnName("PaisID");
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Destino).WithMany(p => p.Tours)
                .HasForeignKey(d => d.DestinoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tour__DestinoID__3D5E1FD2");

            entity.HasOne(d => d.Pais).WithMany(p => p.Tours)
                .HasForeignKey(d => d.PaisId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tour__PaisID__3C69FB99");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}