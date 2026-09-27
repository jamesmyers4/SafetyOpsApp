using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafetyOps.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgUnitsAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrgUnitId",
                table: "TrainingClasses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "OrgUnitId",
                table: "People",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "OrgUnitId",
                table: "Incidents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "OrgUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrgUnits_OrgUnits_ParentId",
                        column: x => x.ParentId,
                        principalTable: "OrgUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrgUnitId = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_OrgUnits_OrgUnitId",
                        column: x => x.OrgUnitId,
                        principalTable: "OrgUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "OrgUnits",
                columns: new[] { "Id", "Code", "Name", "ParentId" },
                values: new object[,]
                {
                    { 1, "ORG", "SafetyOps Industries", null },
                    { 2, "MFG", "Manufacturing Division", 1 },
                    { 3, "LOG", "Logistics Division", 1 },
                    { 4, "MFG-N", "North Plant", 2 },
                    { 5, "MFG-S", "South Plant", 2 },
                    { 6, "LOG-E", "East Warehouse", 3 },
                    { 7, "LOG-W", "West Warehouse", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingClasses_OrgUnitId",
                table: "TrainingClasses",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_People_OrgUnitId",
                table: "People",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_OrgUnitId",
                table: "Incidents",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnits_Code",
                table: "OrgUnits",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnits_ParentId",
                table: "OrgUnits",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_OrgUnitId",
                table: "RoleAssignments",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_UserId_OrgUnitId",
                table: "RoleAssignments",
                columns: new[] { "UserId", "OrgUnitId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_OrgUnits_OrgUnitId",
                table: "Incidents",
                column: "OrgUnitId",
                principalTable: "OrgUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_People_OrgUnits_OrgUnitId",
                table: "People",
                column: "OrgUnitId",
                principalTable: "OrgUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingClasses_OrgUnits_OrgUnitId",
                table: "TrainingClasses",
                column: "OrgUnitId",
                principalTable: "OrgUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_OrgUnits_OrgUnitId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_People_OrgUnits_OrgUnitId",
                table: "People");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingClasses_OrgUnits_OrgUnitId",
                table: "TrainingClasses");

            migrationBuilder.DropTable(
                name: "RoleAssignments");

            migrationBuilder.DropTable(
                name: "OrgUnits");

            migrationBuilder.DropIndex(
                name: "IX_TrainingClasses_OrgUnitId",
                table: "TrainingClasses");

            migrationBuilder.DropIndex(
                name: "IX_People_OrgUnitId",
                table: "People");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_OrgUnitId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "OrgUnitId",
                table: "TrainingClasses");

            migrationBuilder.DropColumn(
                name: "OrgUnitId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "OrgUnitId",
                table: "Incidents");
        }
    }
}
