using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace TG_Bot.monitoring
{
    public partial class _4stasContext : DbContext
    {
        private IConfiguration _configuration;

        private IConfiguration Configuration
        {
            get
            {
                //var scope = serviceProvider.CreateScope();
                //var scopedProvider = scope.ServiceProvider;
                //var config = scopedProvider.GetRequiredService<IConfiguration>();

                return _configuration ??= new ConfigurationBuilder()
                    .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
        }

        public _4stasContext()
        {
        }

        public _4stasContext(DbContextOptions<_4stasContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public virtual DbSet<Monitor> Monitor { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(_configuration.GetConnectionString("DefaultConnection"));
                //optionsBuilder.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Monitor>(entity =>
            {
                entity.ToTable("monitor");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.ClientIp)
                    .HasColumnName("client_ip")
                    .HasColumnType("varchar(20)")
                    .HasDefaultValueSql("''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Phase1)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.PhaseSumm)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.Heat)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.Boiler)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.Energy)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.D14)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.Phase2)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.Phase3)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.TemperatureLivingRoom)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.HumidityLivingRoom)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.TemperatureOutside)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.TemperatureBarn)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.TemperatureBedroom)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.HumidityBedroom)
                    .HasColumnType("float(9,3)")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.Timestamp)
                    .HasColumnName("date_time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'0000-00-00 00:00:00'");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
