using Microsoft.EntityFrameworkCore;
using ODataWebserver.Global;
using ODataWebserver.Models;

namespace ODataWebserver.Webserver
{
    public class DummyContext : DbContext
    {
        public DummyContext(DbContextOptions<DummyContext> options) : base(options)
        {

        }

        public DbSet<ApiConsumer> ApiConsumers { get; set; }
        public DbSet<ApiConsumerLog> ApiConsumerLogs { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobResult> JobResults { get; set; }
        public DbSet<ValueOverride> ValuesToOverwrite { get; set; }
        public DbSet<HyperParameter> HyperParameters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // add custom constraints here nameof(HyperParameterForJob),
            modelBuilder.Entity(NameOfFull<HyperParameterForJob>(), b =>
            {
                b.HasOne(NameOfFull<HyperParameter>(), nameof(HyperParameter))
                    .WithMany()
                    .HasForeignKey(nameof(HyperParameterForJob.HyperParameterId))
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(NameOfFull<Job>(), nameof(Job))
                    .WithMany(nameof(Job.HyperParameters))
                    .HasForeignKey(nameof(HyperParameterForJob.JobId))
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity(NameOfFull<ValueOverride>(), b =>
            {
                b.HasOne(NameOfFull<Job>(), nameof(Job))
                    .WithMany(nameof(Job.ValueOverrides))
                    .HasForeignKey(nameof(ValueOverride.JobId))
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
        private string NameOfFull<T>() where T : IModel => typeof(T).FullName;
    }
}