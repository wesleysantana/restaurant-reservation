using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantReservation.Infra.Migrations
{
    /// <inheritdoc />
    public partial class BusinessHourRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "numberofpeople",
                table: "reservations");

            migrationBuilder.AlterColumn<short>(
                name: "capacity",
                table: "tables",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<short>(
                name: "numberofguests",
                table: "reservations",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "business_hours_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    start_date = table.Column<string>(type: "text", nullable: false),
                    end_date = table.Column<string>(type: "text", nullable: false),
                    specific_date = table.Column<string>(type: "text", nullable: true),
                    week_day = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    open_time = table.Column<string>(type: "text", nullable: true),
                    close_time = table.Column<string>(type: "text", nullable: true),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_business_hours_rules", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_business_hours_day_of_week",
                table: "business_hours_rules",
                column: "week_day");

            migrationBuilder.CreateIndex(
                name: "ix_business_hours_end_date",
                table: "business_hours_rules",
                column: "end_date");

            migrationBuilder.CreateIndex(
                name: "ix_business_hours_specific_date",
                table: "business_hours_rules",
                column: "specific_date");

            migrationBuilder.CreateIndex(
                name: "ix_business_hours_start_date",
                table: "business_hours_rules",
                column: "start_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "business_hours_rules");

            migrationBuilder.DropColumn(
                name: "numberofguests",
                table: "reservations");

            migrationBuilder.AlterColumn<int>(
                name: "capacity",
                table: "tables",
                type: "integer",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<int>(
                name: "numberofpeople",
                table: "reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
