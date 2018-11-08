using FluentMigrator;

namespace api.Migrations
{
    [Migration(201811011506)]
    public class InitialSchema : Migration
    {
        public override void Down()
        {
            Delete.Table("comments");
        }

        public override void Up()
        {
            Create.Table("comments")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("body").AsString()
                .WithColumn("user").AsString();
        }
    }
}