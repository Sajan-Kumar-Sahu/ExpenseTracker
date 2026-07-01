using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderNotificationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderType = table.Column<int>(type: "integer", nullable: false),
                    ReferenceModule = table.Column<int>(type: "integer", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ScheduledDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RepeatType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RepeatInterval = table.Column<int>(type: "integer", nullable: true),
                    IsPushNotificationEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsInAppNotificationEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastTriggeredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    NextTriggerAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NotificationType = table.Column<int>(type: "integer", nullable: false),
                    ReferenceModule = table.Column<int>(type: "integer", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsClicked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ClickedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeliveryStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ActionUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Reminders_ReminderId",
                        column: x => x.ReminderId,
                        principalTable: "Reminders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReminderId",
                table: "Notifications",
                column: "ReminderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_SentAt",
                table: "Notifications",
                columns: new[] { "UserId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_ReferenceModule_ReferenceId",
                table: "Reminders",
                columns: new[] { "ReferenceModule", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_ScheduledDate",
                table: "Reminders",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_Status",
                table: "Reminders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId",
                table: "Reminders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId_ReminderType_ReferenceModule_ReferenceId",
                table: "Reminders",
                columns: new[] { "UserId", "ReminderType", "ReferenceModule", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId_Status",
                table: "Reminders",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Reminders");
        }
    }
}
