using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafetyOps.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EmployeeCategory = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Subscription = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EmployeeNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stressors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stressors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ExamTypeOptions = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingClasses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicalAppointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PersonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalAppointments_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkTaskStressor",
                columns: table => new
                {
                    WorkTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    StressorId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTaskStressor", x => new { x.WorkTaskId, x.StressorId });
                    table.ForeignKey(
                        name: "FK_WorkTaskStressor_Stressors_StressorId",
                        column: x => x.StressorId,
                        principalTable: "Stressors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkTaskStressor_WorkTasks_WorkTaskId",
                        column: x => x.WorkTaskId,
                        principalTable: "WorkTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentStressors",
                columns: table => new
                {
                    AppointmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    StressorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExamType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentStressors", x => new { x.AppointmentId, x.StressorId });
                    table.ForeignKey(
                        name: "FK_AppointmentStressors_MedicalAppointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "MedicalAppointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentStressors_Stressors_StressorId",
                        column: x => x.StressorId,
                        principalTable: "Stressors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Code", "Title" },
                values: new object[,]
                {
                    { 1, "ELV-001", "Electrical - Low Voltage" },
                    { 2, "ELH-001", "Electrical - High Voltage" },
                    { 3, "ELS-001", "Electrical - Safety Basics" },
                    { 4, "FPS-001", "Fire Prevention and Safety" },
                    { 5, "HAZ-001", "Hazardous Materials Handling" },
                    { 6, "PPE-001", "Personal Protective Equipment" },
                    { 7, "FAC-001", "First Aid and CPR" },
                    { 8, "LOT-001", "Lockout/Tagout Procedures" }
                });

            migrationBuilder.InsertData(
                table: "Stressors",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "STR-001", "Solvent Exposure" },
                    { 2, "STR-002", "Noise Exposure" },
                    { 3, "STR-003", "Dust Inhalation" }
                });

            migrationBuilder.InsertData(
                table: "WorkTasks",
                columns: new[] { "Id", "Code", "ExamTypeOptions", "Name" },
                values: new object[,]
                {
                    { 1, "WT-001", "[\"Initial\",\"Periodic\",\"Exit\",\"Return to Duty\"]", "Chemical Exposure - Solvents" },
                    { 2, "WT-002", "[\"Initial\",\"Periodic\",\"Exit\"]", "Noise Hazard - Industrial" },
                    { 3, "WT-003", "[\"Initial\",\"Periodic\",\"Exit\",\"Special\"]", "Respiratory Hazard - Dust" }
                });

            migrationBuilder.InsertData(
                table: "WorkTaskStressor",
                columns: new[] { "StressorId", "WorkTaskId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentStressors_StressorId",
                table: "AppointmentStressors",
                column: "StressorId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Code",
                table: "Courses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalAppointments_PersonId",
                table: "MedicalAppointments",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_People_LastName_FirstName",
                table: "People",
                columns: new[] { "LastName", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_Stressors_Code",
                table: "Stressors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingClasses_CourseId_ClassDate",
                table: "TrainingClasses",
                columns: new[] { "CourseId", "ClassDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_Code",
                table: "WorkTasks",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkTaskStressor_StressorId",
                table: "WorkTaskStressor",
                column: "StressorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentStressors");

            migrationBuilder.DropTable(
                name: "TrainingClasses");

            migrationBuilder.DropTable(
                name: "WorkTaskStressor");

            migrationBuilder.DropTable(
                name: "MedicalAppointments");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Stressors");

            migrationBuilder.DropTable(
                name: "WorkTasks");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
