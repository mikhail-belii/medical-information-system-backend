﻿// <auto-generated />
using System;
using BusinessLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BusinessLogic.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241020170346_ChangedIcd2.0")]
    partial class ChangedIcd20
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0-rc.2.24474.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Common.DbModels.CommentEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuthorId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ConsultationId")
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("ConsultationId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("Common.DbModels.ConsultationEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("InspectionEntityId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("InspectionId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SpecialityId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("InspectionEntityId");

                    b.HasIndex("SpecialityId");

                    b.ToTable("Consultations");
                });

            modelBuilder.Entity("Common.DbModels.DiagnosisEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<Guid>("Icd10Id")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("InspectionEntityId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Icd10Id");

                    b.HasIndex("InspectionEntityId");

                    b.ToTable("Diagnoses");
                });

            modelBuilder.Entity("Common.DbModels.DoctorEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.Property<Guid>("Speciality")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("Doctors");
                });

            modelBuilder.Entity("Common.DbModels.Icd10Entity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<string>("IcdId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("IcdParentId")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Icd10s");
                });

            modelBuilder.Entity("Common.DbModels.InspectionEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Anamnesis")
                        .HasColumnType("text");

                    b.Property<Guid?>("BaseInspectionId")
                        .HasColumnType("uuid");

                    b.Property<string>("Complaints")
                        .HasColumnType("text");

                    b.Property<string>("Conclusion")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DeathDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DoctorId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("NextVisitDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("PatientId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("PreviousInspectionId")
                        .HasColumnType("uuid");

                    b.Property<string>("Treatment")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DoctorId");

                    b.HasIndex("PatientId");

                    b.ToTable("Inspections");
                });

            modelBuilder.Entity("Common.DbModels.PatientEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Patients");
                });

            modelBuilder.Entity("Common.DbModels.SpecialityEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Specialities");
                });

            modelBuilder.Entity("Common.DbModels.CommentEntity", b =>
                {
                    b.HasOne("Common.DbModels.DoctorEntity", "Author")
                        .WithMany("Comments")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Common.DbModels.ConsultationEntity", "Consultation")
                        .WithMany("Comments")
                        .HasForeignKey("ConsultationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("Consultation");
                });

            modelBuilder.Entity("Common.DbModels.ConsultationEntity", b =>
                {
                    b.HasOne("Common.DbModels.InspectionEntity", null)
                        .WithMany("Consultations")
                        .HasForeignKey("InspectionEntityId");

                    b.HasOne("Common.DbModels.SpecialityEntity", "Speciality")
                        .WithMany()
                        .HasForeignKey("SpecialityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Speciality");
                });

            modelBuilder.Entity("Common.DbModels.DiagnosisEntity", b =>
                {
                    b.HasOne("Common.DbModels.Icd10Entity", "Icd10")
                        .WithMany()
                        .HasForeignKey("Icd10Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Common.DbModels.InspectionEntity", null)
                        .WithMany("Diagnoses")
                        .HasForeignKey("InspectionEntityId");

                    b.Navigation("Icd10");
                });

            modelBuilder.Entity("Common.DbModels.InspectionEntity", b =>
                {
                    b.HasOne("Common.DbModels.DoctorEntity", "Doctor")
                        .WithMany("Inspections")
                        .HasForeignKey("DoctorId");

                    b.HasOne("Common.DbModels.PatientEntity", "Patient")
                        .WithMany()
                        .HasForeignKey("PatientId");

                    b.Navigation("Doctor");

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("Common.DbModels.ConsultationEntity", b =>
                {
                    b.Navigation("Comments");
                });

            modelBuilder.Entity("Common.DbModels.DoctorEntity", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("Inspections");
                });

            modelBuilder.Entity("Common.DbModels.InspectionEntity", b =>
                {
                    b.Navigation("Consultations");

                    b.Navigation("Diagnoses");
                });
#pragma warning restore 612, 618
        }
    }
}
