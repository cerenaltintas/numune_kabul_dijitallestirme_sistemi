using Microsoft.EntityFrameworkCore;
using NumuneKabul.Domain.Entities;

namespace NumuneKabul.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Institution> Institutions { get; set; }
    public DbSet<FormTemplate> FormTemplates { get; set; }
    public DbSet<TemplateField> TemplateFields { get; set; }
    public DbSet<PdfDocument> PdfDocuments { get; set; }
    public DbSet<OcrResult> OcrResults { get; set; }
    public DbSet<ExtractedField> ExtractedFields { get; set; }
    public DbSet<XmlArchive> XmlArchives { get; set; }
    public DbSet<IntegrationJob> IntegrationJobs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Institution>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<FormTemplate>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.Institution)
                .WithMany()
                .HasForeignKey(e => e.InstitutionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TemplateField>(entity =>
        {
            entity.Property(e => e.FieldName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Regex).HasMaxLength(500);
            entity.Property(e => e.DataType).HasMaxLength(50);
            entity.HasOne(e => e.FormTemplate)
                .WithMany(f => f.TemplateFields)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PdfDocument>(entity =>
        {
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Institution)
                .WithMany()
                .HasForeignKey(e => e.InstitutionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.FormTemplate)
                .WithMany()
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OcrResult>(entity =>
        {
            entity.Property(e => e.RawText).IsRequired();
            entity.HasOne(e => e.PdfDocument)
                .WithMany()
                .HasForeignKey(e => e.PdfDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExtractedField>(entity =>
        {
            entity.Property(e => e.FieldName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.PdfDocument)
                .WithMany()
                .HasForeignKey(e => e.PdfDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<XmlArchive>(entity =>
        {
            entity.Property(e => e.XmlContent).IsRequired();
            entity.HasOne(e => e.PdfDocument)
                .WithMany()
                .HasForeignKey(e => e.PdfDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IntegrationJob>(entity =>
        {
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.PdfDocument)
                .WithMany()
                .HasForeignKey(e => e.PdfDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
