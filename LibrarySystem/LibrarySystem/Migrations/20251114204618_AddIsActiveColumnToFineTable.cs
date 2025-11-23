using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibrarySystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveColumnToFineTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "05feb88a-88ea-4aff-b8ee-9687f44b3d9d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "dc2d106b-3d4d-4088-8627-ee4d1604b208");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Fines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "284a6fb5-abf2-40f7-a611-6d373faf469d", null, "Admin", "ADMIN" },
                    { "e01e36a0-cc7b-4f0b-b032-aee38c6accc7", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "284a6fb5-abf2-40f7-a611-6d373faf469d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e01e36a0-cc7b-4f0b-b032-aee38c6accc7");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Fines");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "05feb88a-88ea-4aff-b8ee-9687f44b3d9d", null, "User", "USER" },
                    { "dc2d106b-3d4d-4088-8627-ee4d1604b208", null, "Admin", "ADMIN" }
                });
        }
    }
}
