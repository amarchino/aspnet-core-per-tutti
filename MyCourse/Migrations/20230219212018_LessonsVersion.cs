using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCourse.Migrations
{
    public partial class LessonsVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RowVersion",
                table: "Lessons",
                nullable: true);
            migrationBuilder.Sql(@"CREATE TRIGGER LessonsSetRowVersionOnInsert
                                   AFTER INSERT ON Lessons
                                   BEGIN
                                   UPDATE Lessons SET RowVersion = CURRENT_TIMESTAMP WHERE Id=NEW.Id;
                                   END;");
            migrationBuilder.Sql(@"CREATE TRIGGER LessonsSetRowVersionOnUpdate
                                   AFTER UPDATE ON Lessons WHEN NEW.RowVersion <= OLD.RowVersion
                                   BEGIN
                                   UPDATE Lessons SET RowVersion = CURRENT_TIMESTAMP WHERE Id=NEW.Id;
                                   END;");
            migrationBuilder.Sql("UPDATE Lessons SET RowVersion = CURRENT_TIMESTAMP;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER LessonsSetRowVersionOnInsert;");
            migrationBuilder.Sql("DROP TRIGGER LessonsSetRowVersionOnUpdate;");
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Lessons");
        }
    }
}
