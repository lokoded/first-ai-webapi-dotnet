using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComicTypeUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ComicTypes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ComicTypes");
        }
    }
}
