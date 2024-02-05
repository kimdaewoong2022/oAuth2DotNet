using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Movies.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedMovieTableToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Movie",
                columns: new[] { "Id", "Genre", "ImageUrl", "Owner", "Rating", "ReleaseDate", "Title" },
                values: new object[,]
                {
                    { 1, "Drama", "images/src", "alice", "9.3", new DateTime(1994, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "The Shawshank Redemption" },
                    { 2, "Crime", "images/src", "alice", "9.2", new DateTime(1972, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "The Godfather" },
                    { 3, "Action", "images/src", "bob", "9.1", new DateTime(2008, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "The Dark Knight" },
                    { 4, "Crime", "images/src", "bob", "8.9", new DateTime(1957, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "12 Angry Men" },
                    { 5, "Biography", "images/src", "alice", "8.9", new DateTime(1993, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Schindler's List" },
                    { 6, "Drama", "images/src", "alice", "8.9", new DateTime(1994, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pulp Fiction" },
                    { 7, "Drama", "images/src", "bob", "8.8", new DateTime(1999, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fight Club" },
                    { 8, "Romance", "images/src", "bob", "8.8", new DateTime(1994, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Forrest Gump" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Movie",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Movie",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Movie",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Movie",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Movie",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Movie",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Movie",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Movie",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
