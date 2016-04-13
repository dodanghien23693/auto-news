namespace auto_news.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AutoNewsDbContext : DbContext
    {
        public AutoNewsDbContext()
            : base("name=DefaultConnection")
        {
        }

        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CrawlConfig> CrawlConfigs { get; set; }
        public virtual DbSet<NewsSource> NewsSources { get; set; }
        public virtual DbSet<UserSourceConfig> UserSourceConfigs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>()
                .Property(e => e.ImageUrl)
                .IsUnicode(true);

            modelBuilder.Entity<Article>()
                .Property(e => e.RawContent)
                .IsUnicode(true);

            modelBuilder.Entity<Article>()
                .Property(e => e.FormatedContent)
                .IsUnicode(true);

            modelBuilder.Entity<AspNetRole>()
                .HasMany(e => e.AspNetUsers)
                .WithMany(e => e.AspNetRoles)
                .Map(m => m.ToTable("AspNetUserRoles").MapLeftKey("RoleId").MapRightKey("UserId"));

            //modelBuilder.Entity<Category>()
            //    .HasMany(e => e.Articles)
            //    .WithRequired(e => e.Category)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<CrawlConfig>()
                .Property(e => e.Description)
                .IsUnicode(true);

            //modelBuilder.Entity<NewsSource>()
            //    .HasMany(e => e.Articles)
            //    .WithRequired(e => e.NewsSource)
            //    .WillCascadeOnDelete(false);
        }
    }
}
