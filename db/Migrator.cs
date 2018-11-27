using System;
using System.Linq;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

namespace db
{
    public class Migrator
    {
        private readonly MigrationRunner _runner;

        #pragma warning disable 0612
        public Migrator(string connectionString)
        {
            var announcer = new ConsoleAnnouncer()
            {
                ShowSql = true,
            };

            var options = new ProcessorOptions()
            {
                ConnectionString = connectionString,
            };

            var factory = new FluentMigrator.Runner.Processors.MySql.MySql5ProcessorFactory();
            var processor = factory.Create(connectionString, announcer, options);

            var context = new RunnerContext(announcer)
            {
                AllowBreakingChange = true,
            };

            _runner = new MigrationRunner(
                typeof(Migrator).Assembly,
                context,
                processor);
        }

        public void MigrateUp() 
        {
            _runner.MigrateUp();
        }

        public void Rollback()
        {
            _runner.Rollback(Int32.MaxValue);
        }
    }
}
