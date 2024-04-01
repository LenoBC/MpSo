using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MpSo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabaseSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    count = table.Column<int>(type: "INTEGER", nullable: false),
                    percentage_share = table.Column<double>(type: "REAL", nullable: false),
                    has_synonyms = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_required = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_moderator_only = table.Column<bool>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tags");
        }
    }
}
