using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sample.Components.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(nullable: false),
                    CurrentState = table.Column<string>(maxLength: 64, nullable: true),
                    CustomerNumber = table.Column<string>(nullable: true),
                    PaymentCardNumber = table.Column<string>(nullable: true),
                    FaultReason = table.Column<string>(nullable: true),
                    SubmitDate = table.Column<DateTime>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderState", x => x.CorrelationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderState");
        }
    }
}
