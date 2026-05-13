using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftScheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixShiftRequestCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSenior",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RemainingChangeRequests",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "ShiftLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ListType",
                table: "ShiftLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequests_RequesterId",
                table: "ShiftRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequests_ShiftId",
                table: "ShiftRequests",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequests_TargetDoctorId",
                table: "ShiftRequests",
                column: "TargetDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftLists_DepartmentId",
                table: "ShiftLists",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftLists_Departments_DepartmentId",
                table: "ShiftLists",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftRequests_Shifts_ShiftId",
                table: "ShiftRequests",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftRequests_Users_RequesterId",
                table: "ShiftRequests",
                column: "RequesterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftRequests_Users_TargetDoctorId",
                table: "ShiftRequests",
                column: "TargetDoctorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftLists_Departments_DepartmentId",
                table: "ShiftLists");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftRequests_Shifts_ShiftId",
                table: "ShiftRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftRequests_Users_RequesterId",
                table: "ShiftRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ShiftRequests_Users_TargetDoctorId",
                table: "ShiftRequests");

            migrationBuilder.DropIndex(
                name: "IX_ShiftRequests_RequesterId",
                table: "ShiftRequests");

            migrationBuilder.DropIndex(
                name: "IX_ShiftRequests_ShiftId",
                table: "ShiftRequests");

            migrationBuilder.DropIndex(
                name: "IX_ShiftRequests_TargetDoctorId",
                table: "ShiftRequests");

            migrationBuilder.DropIndex(
                name: "IX_ShiftLists_DepartmentId",
                table: "ShiftLists");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsSenior",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RemainingChangeRequests",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "ShiftLists");

            migrationBuilder.DropColumn(
                name: "ListType",
                table: "ShiftLists");
        }
    }
}
