using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCompaniesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUser_City",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUser_Name",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUser_PostalCode",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUser_State",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUser_StreetAddress",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationUser_City",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationUser_Name",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationUser_PostalCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationUser_State",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationUser_StreetAddress",
                table: "AspNetUsers");
        }
    }
}
