using System;
using Xunit.Sdk;
using db;

namespace test
{
    public class UseDatabaseAttribute : BeforeAfterTestAttribute
    {
        private readonly Migrator _migrator;

        public UseDatabaseAttribute(string connectionString)
        {
            _migrator = new Migrator(connectionString);
        }

        public override void Before(System.Reflection.MethodInfo methodUnderTest)
        {
            _migrator.MigrateUp();
            base.Before(methodUnderTest);
        }

        public override void After(System.Reflection.MethodInfo methodUnderTest)
        {
            _migrator.Rollback();
            base.Before(methodUnderTest);
        }
    }
}