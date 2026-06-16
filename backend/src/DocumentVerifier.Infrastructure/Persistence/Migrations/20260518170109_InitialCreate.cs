using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentVerifier.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VerificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VerifierIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VerifierUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BlockchainStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EducationalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DocumentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IssuerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BlockchainTransactionHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ContractAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BlockchainNetwork = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationalDocuments_Users_IssuerUserId",
                        column: x => x.IssuerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationalDocuments_DocumentHash",
                table: "EducationalDocuments",
                column: "DocumentHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationalDocuments_IssuerUserId",
                table: "EducationalDocuments",
                column: "IssuerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EducationalDocuments");

            migrationBuilder.DropTable(
                name: "VerificationLogs");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
