using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibrarySystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdColumnToFineTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "284a6fb5-abf2-40f7-a611-6d373faf469d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e01e36a0-cc7b-4f0b-b032-aee38c6accc7");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Fines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "7c683662-9c0c-4217-a6af-3592371f13c3", null, "Admin", "ADMIN" },
                    { "de98d46b-dc9d-45f0-be70-48a02c6f647f", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7c683662-9c0c-4217-a6af-3592371f13c3");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "de98d46b-dc9d-45f0-be70-48a02c6f647f");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Fines");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "284a6fb5-abf2-40f7-a611-6d373faf469d", null, "Admin", "ADMIN" },
                    { "e01e36a0-cc7b-4f0b-b032-aee38c6accc7", null, "User", "USER" }
                });
        }
    }
}
