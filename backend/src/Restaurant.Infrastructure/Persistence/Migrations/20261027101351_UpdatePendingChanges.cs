using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PreBillPrintedAtUtc",
                schema: "pos",
                table: "TableSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorCode",
                schema: "pos",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentGuestCount",
                schema: "pos",
                table: "Tables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Height",
                schema: "pos",
                table: "Tables",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PositionX",
                schema: "pos",
                table: "Tables",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PositionY",
                schema: "pos",
                table: "Tables",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Shape",
                schema: "pos",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Width",
                schema: "pos",
                table: "Tables",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CardSlipNo",
                schema: "pos",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MfsTrxId",
                schema: "pos",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AggregatorCommissionPercent",
                schema: "pos",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AggregatorName",
                schema: "pos",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestCoverCount",
                schema: "pos",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPreBilled",
                schema: "pos",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MushakChallanNo",
                schema: "pos",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SplitParentOrderId",
                schema: "pos",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourseType",
                schema: "pos",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GuestSeatNo",
                schema: "pos",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSentToKitchen",
                schema: "pos",
                table: "OrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "KotTokenNo",
                schema: "pos",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FloorNumber",
                schema: "pos",
                table: "DiningAreas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Count10",
                schema: "finance",
                table: "CashShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Count100",
                schema: "finance",
                table: "CashShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Count1000",
                schema: "finance",
                table: "CashShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Count20",
                schema: "finance",
                table: "CashShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Count200",
                schema: "finance",
                table: "CashShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Count50",
                schema: "finance",
                table: "CashShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Count500",
                schema: "finance",
                table: "CashShifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DeclaredCash",
                schema: "finance",
                table: "CashShifts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreBillPrintedAtUtc",
                schema: "pos",
                table: "TableSessions");

            migrationBuilder.DropColumn(
                name: "ColorCode",
                schema: "pos",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "CurrentGuestCount",
                schema: "pos",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Height",
                schema: "pos",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "PositionX",
                schema: "pos",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "PositionY",
                schema: "pos",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Shape",
                schema: "pos",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Width",
                schema: "pos",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "CardSlipNo",
                schema: "pos",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "MfsTrxId",
                schema: "pos",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AggregatorCommissionPercent",
                schema: "pos",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AggregatorName",
                schema: "pos",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestCoverCount",
                schema: "pos",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsPreBilled",
                schema: "pos",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MushakChallanNo",
                schema: "pos",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SplitParentOrderId",
                schema: "pos",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CourseType",
                schema: "pos",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "GuestSeatNo",
                schema: "pos",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "IsSentToKitchen",
                schema: "pos",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "KotTokenNo",
                schema: "pos",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "FloorNumber",
                schema: "pos",
                table: "DiningAreas");

            migrationBuilder.DropColumn(
                name: "Count10",
                schema: "finance",
                table: "CashShifts");

            migrationBuilder.DropColumn(
                name: "Count100",
                schema: "finance",
                table: "CashShifts");

            migrationBuilder.DropColumn(
                name: "Count1000",
                schema: "finance",
                table: "CashShifts");

            migrationBuilder.DropColumn(
                name: "Count20",
                schema: "finance",
                table: "CashShifts");

            migrationBuilder.DropColumn(
                name: "Count200",
                schema: "finance",
                table: "CashShifts");

            migrationBuilder.DropColumn(
                name: "Count50",
                schema: "finance",
                table: "CashShifts");

            migrationBuilder.DropColumn(
                name: "Count500",
                schema: "finance",
                table: "CashShifts");

            migrationBuilder.DropColumn(
                name: "DeclaredCash",
                schema: "finance",
                table: "CashShifts");
        }
    }
}
