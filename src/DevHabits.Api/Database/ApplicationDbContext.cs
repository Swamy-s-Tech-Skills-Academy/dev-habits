﻿using DevHabits.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabits.Api.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<HabitTag> HabitTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
