using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class InitialSpecialities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Specialities",
                columns: new[] { "Id", "CreateTime", "Name" },
                values: new object[,]
                {
                    { Guid.NewGuid(), DateTime.UtcNow, "Акушер-гинеколог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Анестезиолог-реаниматолог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Дерматовенеролог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Инфекционист" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Кардиолог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Невролог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Онколог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Отоларинголог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Офтальмолог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Психиатр" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Психолог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Рентгенолог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Стоматолог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Терапевт" },
                    { Guid.NewGuid(), DateTime.UtcNow, "УЗИ-специалист" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Уролог" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Хирург" },
                    { Guid.NewGuid(), DateTime.UtcNow, "Эндокринолог" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
