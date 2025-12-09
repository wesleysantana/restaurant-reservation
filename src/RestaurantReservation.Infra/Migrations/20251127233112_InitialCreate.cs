using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace RestaurantReservation.Infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tables",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "now()"),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tables", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_id = table.Column<Guid>(type: "uuid", nullable: false),
                    starts_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    numberofpeople = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_reservations_tables_table_id",
                        column: x => x.table_id,
                        principalTable: "tables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_reservations_table_time",
                table: "reservations",
                columns: new[] { "table_id", "starts_at", "ends_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reservations_starts_at",
                table: "reservations",
                column: "starts_at");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_status",
                table: "reservations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_table_id",
                table: "reservations",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_user_id",
                table: "reservations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tables_active",
                table: "tables",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "ix_tables_name_unique",
                table: "tables",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tables_status",
                table: "tables",
                column: "status");

            // -----------------------------------------------
            // Criação manual
            // Habilitar extensões necessárias
            // Contraints CHECK, time_range e GIST no-overlap
            // -----------------------------------------------
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");

            // Constraints CHECK para tables
            migrationBuilder.Sql(@"
                ALTER TABLE tables
                ADD CONSTRAINT ck_tables_capacity
                CHECK (capacity > 0);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE tables
                ADD CONSTRAINT ck_tables_status
                CHECK (status IN ('Disponivel','Reservada','Inativa'));
            ");

            // Constraints CHECK para reservations
            migrationBuilder.Sql(@"
                ALTER TABLE reservations
                ADD CONSTRAINT ck_reservations_ends_after_starts
                CHECK (ends_at > starts_at);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE reservations
                ADD CONSTRAINT ck_reservations_status
                CHECK (status IN ('Ativo','Cancelado'));
            ");

            // Adicionar coluna time_range gerada
            migrationBuilder.Sql(@"
                ALTER TABLE reservations
                ADD COLUMN time_range tstzrange
                GENERATED ALWAYS AS (tstzrange(starts_at, ends_at, '[)')) STORED;
            ");

            // Restrição de não sobreposição (no-overlap) por mesa
            migrationBuilder.Sql(@"
                ALTER TABLE reservations
                ADD CONSTRAINT uniq_reservations_no_overlap
                EXCLUDE USING gist
                (
                    table_id WITH =,
                    time_range WITH &&
                )
                WHERE (status = 'Ativo');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover constraints e índices
            migrationBuilder.Sql(@"
                ALTER TABLE reservations
                DROP CONSTRAINT IF EXISTS uniq_reservations_no_overlap;
            ");

            migrationBuilder.Sql("ALTER TABLE reservations DROP CONSTRAINT IF EXISTS ck_reservations_status;");
            migrationBuilder.Sql("ALTER TABLE reservations DROP CONSTRAINT IF EXISTS ck_reservations_ends_after_starts;");
            migrationBuilder.Sql("ALTER TABLE tables DROP CONSTRAINT IF EXISTS ck_tables_status;");
            migrationBuilder.Sql("ALTER TABLE tables DROP CONSTRAINT IF EXISTS ck_tables_capacity;");

            migrationBuilder.Sql("ALTER TABLE reservations DROP COLUMN IF EXISTS time_range;");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "tables");
        }
    }
}
