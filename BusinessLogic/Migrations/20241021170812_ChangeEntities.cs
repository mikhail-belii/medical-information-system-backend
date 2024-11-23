using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessLogic.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Consultations_ConsultationId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Doctors_AuthorId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Specialities_SpecialityId",
                table: "Consultations");

            migrationBuilder.DropForeignKey(
                name: "FK_Diagnoses_Icd10s_Icd10Id",
                table: "Diagnoses");

            migrationBuilder.DropIndex(
                name: "IX_Diagnoses_Icd10Id",
                table: "Diagnoses");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_SpecialityId",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ConsultationId",
                table: "Comments");

            migrationBuilder.AddColumn<Guid>(
                name: "ConsultationEntityId",
                table: "Comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorEntityId",
                table: "Comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ConsultationEntityId",
                table: "Comments",
                column: "ConsultationEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_DoctorEntityId",
                table: "Comments",
                column: "DoctorEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Consultations_ConsultationEntityId",
                table: "Comments",
                column: "ConsultationEntityId",
                principalTable: "Consultations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Doctors_DoctorEntityId",
                table: "Comments",
                column: "DoctorEntityId",
                principalTable: "Doctors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Consultations_ConsultationEntityId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Doctors_DoctorEntityId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ConsultationEntityId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_DoctorEntityId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ConsultationEntityId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "DoctorEntityId",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_Icd10Id",
                table: "Diagnoses",
                column: "Icd10Id");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_SpecialityId",
                table: "Consultations",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ConsultationId",
                table: "Comments",
                column: "ConsultationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Consultations_ConsultationId",
                table: "Comments",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Doctors_AuthorId",
                table: "Comments",
                column: "AuthorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Specialities_SpecialityId",
                table: "Consultations",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Diagnoses_Icd10s_Icd10Id",
                table: "Diagnoses",
                column: "Icd10Id",
                principalTable: "Icd10s",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
