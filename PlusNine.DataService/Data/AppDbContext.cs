﻿using Microsoft.EntityFrameworkCore;
using PlusNine.Entities.DbSet;

namespace PlusNine.DataService.Data
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<Objective> Objectives { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<FriendRequest> FriendRequests { get; set; }
        public virtual DbSet<Friendship> Frienships { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

        }
    }
}
