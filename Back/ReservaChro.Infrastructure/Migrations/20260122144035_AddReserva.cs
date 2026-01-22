using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaChro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessorId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataReserva = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HorarioInicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    HorarioFim = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Users_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ProfessorId",
                table: "Reservas",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_SchoolId_DataReserva_Status",
                table: "Reservas",
                columns: new[] { "SchoolId", "DataReserva", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservas");
        }
    }
}
