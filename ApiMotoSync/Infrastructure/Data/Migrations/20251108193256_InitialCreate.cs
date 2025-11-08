using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiMotoSync.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FILIAIS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Nome = table.Column<string>(type: "NVARCHAR2(160)", maxLength: 160, nullable: false),
                    Codigo = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Endereco = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Cidade = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    Estado = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FILIAIS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "USUARIOS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Nome = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(180)", maxLength: 180, nullable: false),
                    Cargo = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    FilialId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_USUARIOS_FILIAIS_FilialId",
                        column: x => x.FilialId,
                        principalTable: "FILIAIS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MOTOS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Modelo = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    Placa = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    Ano = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false, defaultValue: "Disponível"),
                    FilialId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    GestorId = table.Column<Guid>(type: "RAW(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOTOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MOTOS_FILIAIS_FilialId",
                        column: x => x.FilialId,
                        principalTable: "FILIAIS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MOTOS_USUARIOS_GestorId",
                        column: x => x.GestorId,
                        principalTable: "USUARIOS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FILIAIS_Codigo",
                table: "FILIAIS",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MOTOS_FilialId",
                table: "MOTOS",
                column: "FilialId");

            migrationBuilder.CreateIndex(
                name: "IX_MOTOS_GestorId",
                table: "MOTOS",
                column: "GestorId");

            migrationBuilder.CreateIndex(
                name: "IX_MOTOS_Placa",
                table: "MOTOS",
                column: "Placa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIOS_Email",
                table: "USUARIOS",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIOS_FilialId",
                table: "USUARIOS",
                column: "FilialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MOTOS");

            migrationBuilder.DropTable(
                name: "USUARIOS");

            migrationBuilder.DropTable(
                name: "FILIAIS");
        }
    }
}
