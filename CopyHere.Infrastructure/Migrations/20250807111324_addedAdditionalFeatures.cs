using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CopyHere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedAdditionalFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ClipboardEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "ClipboardEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "ClipboardEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ClipboardEntries");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "ClipboardEntries");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "ClipboardEntries");
        }
    }
}
