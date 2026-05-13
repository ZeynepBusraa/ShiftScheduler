using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftScheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftListAndRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Shifts;");
            
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Shifts");

            migrationBuilder.AddColumn<int>(
                name: "ShiftListId",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ShiftLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PreparedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftLists_Users_PreparedByUserId",
                        column: x => x.PreparedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_ShiftListId",
                table: "Shifts",
                column: "ShiftListId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftLists_PreparedByUserId",
                table: "ShiftLists",
                column: "PreparedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_ShiftLists_ShiftListId",
                table: "Shifts",
                column: "ShiftListId",
                principalTable: "ShiftLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_ShiftLists_ShiftListId",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "ShiftLists");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_ShiftListId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "ShiftListId",
                table: "Shifts");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Shifts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
