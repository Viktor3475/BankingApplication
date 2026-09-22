using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMoneyMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "money_movements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_money_movements", x => x.Id);
                    table.CheckConstraint("ck_money_movements_amount_positive", "\"Amount\" > 0");
                    table.CheckConstraint("ck_money_movements_distinct_accounts", "\"SourceAccountId\" <> \"DestinationAccountId\"");
                    table.ForeignKey(
                        name: "FK_money_movements_accounts_DestinationAccountId",
                        column: x => x.DestinationAccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_money_movements_accounts_SourceAccountId",
                        column: x => x.SourceAccountId,
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_money_movements_CreatedAtUtc",
                table: "money_movements",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_money_movements_DestinationAccountId",
                table: "money_movements",
                column: "DestinationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_money_movements_SourceAccountId",
                table: "money_movements",
                column: "SourceAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "money_movements");
        }
    }
}
