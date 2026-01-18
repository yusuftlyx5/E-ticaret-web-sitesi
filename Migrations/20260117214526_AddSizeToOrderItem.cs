using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EticaretApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSizeToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "OrderItems");
        }
    }
}
