using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
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

        public virtual DbSet<Ccu> CCU { get; set; }

        public virtual DbSet<Openweather> Weather { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = Configuration.GetConnectionString("DefaultConnection");
                ServerVersion version = ServerVersion.AutoDetect(connectionString);
                optionsBuilder.UseMySql(connectionString, version);
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
                    .UseCollation("utf8_general_ci");

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

                entity.Property(e => e.BedroomYouth)
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

            modelBuilder.Entity<Ccu>(entity =>
            {
                entity.ToTable("ccu");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.BattState)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.ClientIp)
                    .HasMaxLength(20)
                    .HasColumnName("client_ip")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.DateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("date_time")
                    .HasDefaultValueSql("'0000-00-00 00:00:00'");

                entity.Property(e => e.DcPower).HasColumnName("DC_Power");

                entity.Property(e => e.In1)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.In2)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.In3)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.In4)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.In5)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.In6)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.In7)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.In8)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.Mode)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Boiler)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.WarmFloorsBath)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.BedroomYouth)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.WarmFloorKitchen)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.O5)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.R1)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");

                entity.Property(e => e.R2)
                    .HasColumnType("float(9,2)")
                    .HasDefaultValueSql("'0.00'");
            });

            modelBuilder.Entity<Openweather>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("openweather");

                entity.HasIndex(e => e.Id, "Id");

                

                entity.Property(e => e.CloudsAll)
                    .HasColumnType("float(9,3)")
                    .HasColumnName("clouds_all")
                    .HasDefaultValueSql("'0.000'");
                

                entity.Property(e => e.DateTime)
                    .HasColumnType("datetime")
                    .HasColumnName("date_time")
                    .HasDefaultValueSql("'0000-00-00 00:00:00'");

                entity.Property(e => e.Dt)
                    .HasColumnType("datetime")
                    .HasColumnName("dt")
                    .HasDefaultValueSql("'0000-00-00 00:00:00'");

                entity.Property(e => e.TemperatureFeelsLike)
                    .HasColumnType("float(9,3)")
                    .HasColumnName("feels_like")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.Humidity)
                    .HasColumnType("float(9,3)")
                    .HasColumnName("humidity")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Pressure)
                    .HasColumnType("float(9,3)")
                    .HasColumnName("pressure")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.Sunrise)
                    .HasColumnType("datetime")
                    .HasColumnName("sunrise")
                    .HasDefaultValueSql("'0000-00-00 00:00:00'");

                entity.Property(e => e.SunriseText)
                    .HasColumnType("int(16)")
                    .HasColumnName("sunrise_text");

                entity.Property(e => e.Sunset)
                    .HasColumnType("datetime")
                    .HasColumnName("sunset")
                    .HasDefaultValueSql("'0000-00-00 00:00:00'");

                entity.Property(e => e.SunsetText)
                    .HasColumnType("int(16)")
                    .HasColumnName("sunset_text");

                entity.Property(e => e.Temperature)
                    .HasColumnType("float(9,3)")
                    .HasColumnName("temp")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.WeatherDescription)
                    .HasMaxLength(50)
                    .HasColumnName("weather_description")
                    .IsFixedLength(true);

                entity.Property(e => e.WeatherId)
                    .HasColumnType("int(16)")
                    .HasColumnName("weather_id");

                entity.Property(e => e.WeatherMain)
                    .HasMaxLength(50)
                    .HasColumnName("weather_main")
                    .IsFixedLength(true);

                entity.Property(e => e.WindDeg)
                    .HasColumnType("float(9,3)")
                    .HasColumnName("wind_deg")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.WindGust)
                    .HasColumnType("float(9,3)")
                    .HasColumnName("wind_gust")
                    .HasDefaultValueSql("'0.000'");

                entity.Property(e => e.WindSpeed)
                    .HasColumnType("float(9,3)")
                    .HasColumnName("wind_speed")
                    .HasDefaultValueSql("'0.000'");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
