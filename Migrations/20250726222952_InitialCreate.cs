using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgenciaDeTours.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pais",
                columns: table => new
                {
                    PaisID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodigoISO = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IdiomaOficial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Continente = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pais__B501E1A532094361", x => x.PaisID);
                });

            migrationBuilder.CreateTable(
                name: "Destino",
                columns: table => new
                {
                    DestinoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaisID = table.Column<int>(type: "int", nullable: false),
                    DuracionDias = table.Column<int>(type: "int", nullable: false),
                    DuracionHoras = table.Column<int>(type: "int", nullable: false),
                    Clima = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TipoDestino = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Actividades = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AtractivoPrincipal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Destino__4A838EF681BB0B48", x => x.DestinoID);
                    table.ForeignKey(
                        name: "FK__Destino__PaisID__398D8EEE",
                        column: x => x.PaisID,
                        principalTable: "Pais",
                        principalColumn: "PaisID");
                });

            migrationBuilder.CreateTable(
                name: "Tour",
                columns: table => new
                {
                    TourID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PaisID = table.Column<int>(type: "int", nullable: false),
                    DestinoID = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ITBIS = table.Column<decimal>(type: "numeric(13,4)", nullable: true, computedColumnSql: "([Precio]*(0.18))", stored: true),
                    DuracionTotalHoras = table.Column<int>(type: "int", nullable: true),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime", nullable: true),
                    Estado = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, computedColumnSql: "(case when [FechaHoraFin]>=getdate() then 'Vigente' else 'No Vigente' end)", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tour__604CEA101A086580", x => x.TourID);
                    table.ForeignKey(
                        name: "FK__Tour__DestinoID__3D5E1FD2",
                        column: x => x.DestinoID,
                        principalTable: "Destino",
                        principalColumn: "DestinoID");
                    table.ForeignKey(
                        name: "FK__Tour__PaisID__3C69FB99",
                        column: x => x.PaisID,
                        principalTable: "Pais",
                        principalColumn: "PaisID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Destino_PaisID",
                table: "Destino",
                column: "PaisID");

            migrationBuilder.CreateIndex(
                name: "IX_Pais_CodigoIso_Unique",
                table: "Pais",
                column: "CodigoISO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tour_DestinoID",
                table: "Tour",
                column: "DestinoID");

            migrationBuilder.CreateIndex(
                name: "IX_Tour_PaisID",
                table: "Tour",
                column: "PaisID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tour");

            migrationBuilder.DropTable(
                name: "Destino");

            migrationBuilder.DropTable(
                name: "Pais");
        }
    }
}
