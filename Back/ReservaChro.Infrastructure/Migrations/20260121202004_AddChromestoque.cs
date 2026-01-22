using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservaChro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChromestoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chromestoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeMaquina = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NumeroSerie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DataAquisicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chromestoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chromestoque_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chromestoque_NumeroSerie",
                table: "Chromestoque",
                column: "NumeroSerie",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chromestoque_SchoolId",
                table: "Chromestoque",
                column: "SchoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chromestoque");
        }
    }
}
