using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreUsuario = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClavePrimaria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValoresAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValoresNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DireccionIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MotivosMerma",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiereEvidencia = table.Column<bool>(type: "bit", nullable: false),
                    RequiereAutorizacion = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotivosMerma", x => x.Id);
                    table.CheckConstraint("CK_MotivosMerma_Estado", "[Estado] IN (1,2)");
                });

            migrationBuilder.CreateTable(
                name: "TiposRegistro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposRegistro", x => x.Id);
                    table.CheckConstraint("CK_TiposRegistro_Estado", "[Estado] IN (1,2)");
                });

            migrationBuilder.CreateTable(
                name: "Trabajadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ModificadoPor = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trabajadores", x => x.Id);
                    table.CheckConstraint("CK_Trabajadores_Estado", "[Estado] IN (1,2)");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Entradas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoRegistroId = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CantidadInicial = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    DocumentoOrigen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    UsuarioResponsableId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FechaUltimoMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnuladaPorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entradas", x => x.Id);
                    table.CheckConstraint("CK_Entradas_Cantidad", "[CantidadInicial] > 0");
                    table.CheckConstraint("CK_Entradas_Estado", "[Estado] IN (1,2)");
                    table.ForeignKey(
                        name: "FK_Entradas_TiposRegistro_TipoRegistroId",
                        column: x => x.TipoRegistroId,
                        principalTable: "TiposRegistro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Asignaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntradaId = table.Column<int>(type: "int", nullable: false),
                    TrabajadorId = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    UsuarioResponsableId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AnuladaPorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asignaciones", x => x.Id);
                    table.CheckConstraint("CK_Asignaciones_Cantidad", "[Cantidad] > 0");
                    table.CheckConstraint("CK_Asignaciones_Estado", "[Estado] IN (1,2)");
                    table.ForeignKey(
                        name: "FK_Asignaciones_Entradas_EntradaId",
                        column: x => x.EntradaId,
                        principalTable: "Entradas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asignaciones_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mermas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntradaId = table.Column<int>(type: "int", nullable: false),
                    MotivoMermaId = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EvidenciaReferencia = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    UsuarioResponsableId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AnuladaPorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mermas", x => x.Id);
                    table.CheckConstraint("CK_Mermas_Cantidad", "[Cantidad] > 0");
                    table.CheckConstraint("CK_Mermas_Estado", "[Estado] IN (1,2)");
                    table.ForeignKey(
                        name: "FK_Mermas_Entradas_EntradaId",
                        column: x => x.EntradaId,
                        principalTable: "Entradas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mermas_MotivosMerma_MotivoMermaId",
                        column: x => x.MotivoMermaId,
                        principalTable: "MotivosMerma",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asignaciones_EntradaId",
                table: "Asignaciones",
                column: "EntradaId");

            migrationBuilder.CreateIndex(
                name: "IX_Asignaciones_Estado",
                table: "Asignaciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Asignaciones_FechaHora",
                table: "Asignaciones",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Asignaciones_TrabajadorId",
                table: "Asignaciones",
                column: "TrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Entidad_ClavePrimaria",
                table: "Auditorias",
                columns: new[] { "Entidad", "ClavePrimaria" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_FechaHora",
                table: "Auditorias",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Entradas_DocumentoOrigen_TipoRegistroId",
                table: "Entradas",
                columns: new[] { "DocumentoOrigen", "TipoRegistroId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entradas_Estado",
                table: "Entradas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Entradas_FechaHora",
                table: "Entradas",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Entradas_TipoRegistroId",
                table: "Entradas",
                column: "TipoRegistroId");

            migrationBuilder.CreateIndex(
                name: "IX_Mermas_EntradaId",
                table: "Mermas",
                column: "EntradaId");

            migrationBuilder.CreateIndex(
                name: "IX_Mermas_Estado",
                table: "Mermas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Mermas_FechaHora",
                table: "Mermas",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Mermas_MotivoMermaId",
                table: "Mermas",
                column: "MotivoMermaId");

            migrationBuilder.CreateIndex(
                name: "IX_MotivosMerma_Nombre",
                table: "MotivosMerma",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposRegistro_Nombre",
                table: "TiposRegistro",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trabajadores_Rut",
                table: "Trabajadores",
                column: "Rut",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asignaciones");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "Mermas");

            migrationBuilder.DropTable(
                name: "Trabajadores");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Entradas");

            migrationBuilder.DropTable(
                name: "MotivosMerma");

            migrationBuilder.DropTable(
                name: "TiposRegistro");
        }
    }
}
