using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoleBasedAuthenticationApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPublicIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "UserPublicIdSequence",
                startValue: 100000L);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                defaultValueSql: "NEXT VALUE FOR UserPublicIdSequence");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PublicId",
                table: "AspNetUsers",
                column: "PublicId",
                unique: true,
                filter: "[PublicId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PublicId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "AspNetUsers");

            migrationBuilder.DropSequence(
                name: "UserPublicIdSequence");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETUTCDATE()");
        }
    }
}
