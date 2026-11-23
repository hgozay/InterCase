using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InterCase.Repository.Migrations
{
    public partial class AddUsersAndLogins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    display_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_demo = table.Column<bool>(type: "boolean", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_public_id",
                table: "users",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "username", "display_name", "password_hash", "is_demo", "public_id", "created_at", "is_deleted" },
                values: new object[,]
                {
                    { "demo01", "Ayşe Yılmaz", "PBKDF2-SHA256$100000$BgEghPutY+swss+PJ40+pA==$4a9YfCeg5O4z7JHVFKXi++DBSqmNzXb6merPIIok7Ps=", true, new Guid("cc2bae55-f897-4f8f-8536-86cea0105ae5"), new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), false },
                    { "demo02", "Mehmet Kaya", "PBKDF2-SHA256$100000$Hu8PWA+mUMMxtvao9cox8g==$16NbezRXeXOP37q9sfjCwV3rrK1DES396Ks7dxIDEbg=", true, new Guid("6036a468-90ea-477e-8a68-58f0efd4988f"), new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), false },
                    { "demo03", "Zeynep Demir", "PBKDF2-SHA256$100000$rly12IWygnGb8mbSPpL07g==$L8G3P38AdkOCBS+u9WS0VMJYGkYpbyo6zly2GdI36gc=", true, new Guid("9383966d-6f42-418a-9d84-c2a7c909963e"), new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), false },
                    { "demo04", "Ahmet Çelik", "PBKDF2-SHA256$100000$v1Admz5y+ub3rVmBar3XcQ==$m1bSinFMjzhR1CG/YqxK71LSzh2b0J7eUEqreQ5qSZg=", true, new Guid("49251cb6-f3e0-40e6-af3a-33d0f5f1d8c8"), new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), false },
                    { "demo05", "Elif Şahin", "PBKDF2-SHA256$100000$CfnAgatHL17zrdDyOfwL5w==$W//M2LDIVG/jjJoiOpb/Ude9Dfzfv2SVYYzyXlBVvAE=", true, new Guid("192a20f9-cbd7-45c4-bb89-4e8819c73162"), new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), false },
                    { "demo06", "Mustafa Yıldız", "PBKDF2-SHA256$100000$dgJW+86YkISr+iGLVu/0ew==$3+9sg5fbHCedOaEHcQNz08vGApW/FQKVl4NgV2jmv+M=", true, new Guid("d74d87ec-6367-4e36-ba12-ddfadc89cd78"), new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), false },
                    { "demo07", "Fatma Öztürk", "PBKDF2-SHA256$100000$WPjyR4aQkF/CRW465uaK1A==$FTGk95PPCYDkZhr8mFEvSIO5cO9VuMcRKsTLMHzJKfE=", true, new Guid("4e48a589-19c5-4139-8d2e-1503ae52d36a"), new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), false },
                    { "demo08", "Emre Aydın", "PBKDF2-SHA256$100000$25p6Dquu93qvAhvRDyAK/w==$N/7MiWKiCcUFwwbDmglt4SUNu3v/J8uEtulyIrLHrqI=", true, new Guid("010b1991-32db-4936-8474-def39ccb7075"), new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), false },
                    { "demo09", "Selin Arslan", "PBKDF2-SHA256$100000$gNI2eD91etCJ/XEUj/1ssg==$Q0RcYXuJQMMCapH0b2R4I+kPqiMFovnRjr7fuxCGA8Y=", true, new Guid("20a58ff7-533b-45b3-bc21-1a27ffc364f9"), new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), false },
                    { "demo10", "Burak Doğan", "PBKDF2-SHA256$100000$WDFp7YttjAT+FZxNowgvNA==$XvAwovHLKlbmk16C+x42R03u8fd8CZdW03+pSyu9GiQ=", true, new Guid("052ef121-628f-4fb2-bd02-504396fd1dcc"), new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc), false }
                });

            migrationBuilder.AddColumn<long>(
                name: "user_ref",
                table: "orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("UPDATE orders SET user_ref = (SELECT id FROM users WHERE username = 'demo01');");

            migrationBuilder.DropIndex(
                name: "ix_orders_user_id_created_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "user_ref",
                table: "orders",
                newName: "user_id");

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "orders",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_id_created_at",
                table: "orders",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.AddColumn<long>(
                name: "user_ref",
                table: "notifications",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("UPDATE notifications n SET user_ref = o.user_id FROM orders o WHERE o.id = n.order_id;");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "notifications");

            migrationBuilder.RenameColumn(
                name: "user_ref",
                table: "notifications",
                newName: "user_id");

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "notifications",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    logged_in_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    token_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    access_token = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_logins", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_logins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_public_id",
                table: "user_logins",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_user_id",
                table: "user_logins",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_users_user_id",
                table: "notifications",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notifications_users_user_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_users_user_id",
                table: "orders");

            migrationBuilder.DropTable(
                name: "user_logins");

            migrationBuilder.AddColumn<Guid>(
                name: "user_uuid",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("UPDATE orders o SET user_uuid = u.public_id FROM users u WHERE u.id = o.user_id;");

            migrationBuilder.DropIndex(
                name: "ix_orders_user_id_created_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "user_uuid",
                table: "orders",
                newName: "user_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_id_created_at",
                table: "orders",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.AddColumn<Guid>(
                name: "user_uuid",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("UPDATE notifications n SET user_uuid = u.public_id FROM users u WHERE u.id = n.user_id;");

            migrationBuilder.DropIndex(
                name: "ix_notifications_user_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "notifications");

            migrationBuilder.RenameColumn(
                name: "user_uuid",
                table: "notifications",
                newName: "user_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "notifications",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
