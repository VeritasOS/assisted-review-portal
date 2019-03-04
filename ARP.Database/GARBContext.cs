/*
Copyright (c) 2019 Veritas Technologies LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using AutoMapper;
using garb.Helpers;
using garb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace garb.Data
{
    public class GarbContext : DbContext
    {
		private readonly IMapper _mapper;

		public GarbContext(DbContextOptions<GarbContext> options, IMapper mapper) : base(options)
        {
			_mapper = mapper;
		}

        public DbSet<Build> Builds { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Screen> Screens { get; set; }
        public DbSet<ScreenInBuild> ScreensInBuilds { get; set; }
        public DbSet<Locale> Locales { get; set; }
        public DbSet<Comparison> Comparisons { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<IssueRevision> IssueRevision { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Build>().ToTable("Builds");
            modelBuilder.Entity<Project>().ToTable("Projects");
            modelBuilder.Entity<Screen>().ToTable("Screens");
            modelBuilder.Entity<ScreenInBuild>().ToTable("ScreensInBuilds");
            modelBuilder.Entity<Locale>().ToTable("Locales");
            modelBuilder.Entity<Comparison>().ToTable("Comparisons");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Issue>().ToTable("Issues");
            modelBuilder.Entity<IssueRevision>().ToTable("IssueRevisions");

            modelBuilder.Entity<Screen>().HasKey(s => new { s.ScreenName, s.ProjectName });
            modelBuilder.Entity<IssueRevision>().HasKey(i => new { i.IssueId, i.RevisionNo });

            modelBuilder.Entity<Build>().HasAlternateKey(b => new { b.ProjectName, b.BuildName });
            modelBuilder.Entity<ScreenInBuild>().HasAlternateKey(s => new { s.ProjectName, s.ScreenName, s.LocaleCode, s.BuildId});

            modelBuilder.Entity<Comparison>().HasKey(c => new { c.SourceScreenInBuildId, c.TargetScreenInBuildId });

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

		private void CheckTrackedEntities(string userName)
		{
			foreach (var entry in ChangeTracker.Entries<ITrackedEntity>().Where(a => a.State == EntityState.Added || a.State == EntityState.Modified))
			{
				if (userName == null)
					throw new Exception("Need to provide user identifier when making changes to tracked entities!");

				entry.Entity.ModificationTime = DateTime.UtcNow;
				entry.Entity.ModifiedByUser = userName;
			}

			foreach (var entry in ChangeTracker.Entries<Issue>().Where(a => a.State == EntityState.Added || a.State == EntityState.Modified).ToList())
			{
				if (entry.State == EntityState.Modified)
				{
					Issue lastRevision = (Issue)entry.GetDatabaseValues().ToObject();

					//Issue lastRevision = EntityHelper.GetOriginal<Issue>(this, entry.Entity);

					IssueRevision newRevision = _mapper.Map<Issue, IssueRevision>(lastRevision);

					int newRevisionNo = 1;

					var item = this.IssueRevision.Where(pr => pr.IssueId == newRevision.IssueId).OrderByDescending(pr => pr.RevisionNo).FirstOrDefault();

					if (item != null)
						newRevisionNo = item.RevisionNo + 1;

					newRevision.RevisionNo = newRevisionNo;

					this.IssueRevision.Add(newRevision);
				}
			}
		}

		public int SaveChanges(string userName = null)
		{
			CheckTrackedEntities(userName);
			return base.SaveChanges();
		}

		public async Task<int> SaveChangesAsync(string userName = null)
		{
			CheckTrackedEntities(userName);
			return await base.SaveChangesAsync();
		}
	}



}
