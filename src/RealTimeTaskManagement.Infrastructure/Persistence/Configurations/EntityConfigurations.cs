using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealTimeTaskManagement.Domain.Entities;
using RealTimeTaskManagement.Infrastructure.Identity;

namespace RealTimeTaskManagement.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.DisplayName).HasMaxLength(100);
    }
}

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasIndex(project => project.Slug).IsUnique();
        builder.Property(project => project.Name).HasMaxLength(120);
        builder.Property(project => project.Slug).HasMaxLength(140);
        builder.Property(project => project.Description).HasMaxLength(500);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(project => project.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasIndex(member => new { member.ProjectId, member.UserId }).IsUnique();
        builder.Property(member => member.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(member => member.Project)
            .WithMany(project => project.Members)
            .HasForeignKey(member => member.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ProjectInviteConfiguration : IEntityTypeConfiguration<ProjectInvite>
{
    public void Configure(EntityTypeBuilder<ProjectInvite> builder)
    {
        builder.HasIndex(invite => invite.CodeHash).IsUnique();
        builder.Property(invite => invite.CodeHash).HasMaxLength(64);
        builder.Property(invite => invite.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(invite => invite.Project)
            .WithMany(project => project.Invites)
            .HasForeignKey(invite => invite.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(invite => invite.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(EntityTypeBuilder<BoardColumn> builder)
    {
        builder.HasIndex(column => new { column.ProjectId, column.SortOrder }).IsUnique();
        builder.HasIndex(column => new { column.ProjectId, column.Key }).IsUnique();
        builder.Property(column => column.Name).HasMaxLength(80);
        builder.Property(column => column.Key).HasMaxLength(40);

        builder.HasOne(column => column.Project)
            .WithMany(project => project.Columns)
            .HasForeignKey(column => column.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasIndex(task => new { task.ProjectId, task.BoardColumnId, task.SortOrder });
        builder.Property(task => task.Title).HasMaxLength(200);
        builder.Property(task => task.Description).HasMaxLength(2000);
        builder.Property(task => task.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(task => task.SortOrder).HasPrecision(18, 6);
        builder.Property(task => task.RowVersion).IsRowVersion();

        builder.HasOne(task => task.Project)
            .WithMany(project => project.Tasks)
            .HasForeignKey(task => task.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(task => task.BoardColumn)
            .WithMany(column => column.Tasks)
            .HasForeignKey(task => task.BoardColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(task => task.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(task => task.AssignedToUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
