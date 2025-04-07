﻿// <auto-generated />
using System;
using HealthDevice.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HealthDevice.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("HealthDevice.DTO.Caregiver", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("text");

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Caregiver");
                });

            modelBuilder.Entity("HealthDevice.DTO.Elder", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("Arduino")
                        .HasColumnType("text");

                    b.Property<string>("CaregiverId")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<int>("LocationId")
                        .HasColumnType("integer");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("text");

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<int?>("PerimeterId")
                        .HasColumnType("integer");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CaregiverId");

                    b.HasIndex("LocationId");

                    b.HasIndex("PerimeterId");

                    b.ToTable("Elder");
                });

            modelBuilder.Entity("HealthDevice.DTO.FallInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ElderId")
                        .HasColumnType("text");

                    b.Property<int>("LocationId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ElderId");

                    b.HasIndex("LocationId");

                    b.ToTable("FallInfo");
                });

            modelBuilder.Entity("HealthDevice.DTO.GPS", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<float>("Course")
                        .HasColumnType("real");

                    b.Property<string>("ElderId")
                        .HasColumnType("text");

                    b.Property<double>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<double>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ElderId");

                    b.ToTable("GPSData");
                });

            modelBuilder.Entity("HealthDevice.DTO.Heartrate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Avgrate")
                        .HasColumnType("integer");

                    b.Property<string>("ElderId")
                        .HasColumnType("text");

                    b.Property<int>("Maxrate")
                        .HasColumnType("integer");

                    b.Property<int>("Minrate")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ElderId");

                    b.ToTable("Heartrate");
                });

            modelBuilder.Entity("HealthDevice.DTO.Kilometer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<double>("Distance")
                        .HasColumnType("double precision");

                    b.Property<string>("ElderId")
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ElderId");

                    b.ToTable("Kilometer");
                });

            modelBuilder.Entity("HealthDevice.DTO.Location", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<double>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<double>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Location");
                });

            modelBuilder.Entity("HealthDevice.DTO.Max30102", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ElderId")
                        .HasColumnType("text");

                    b.Property<int>("Heartrate")
                        .HasColumnType("integer");

                    b.Property<float>("SpO2")
                        .HasColumnType("real");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ElderId");

                    b.ToTable("MAX30102Data");
                });

            modelBuilder.Entity("HealthDevice.DTO.Perimeter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("LocationId")
                        .HasColumnType("integer");

                    b.Property<int>("Radius")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.ToTable("Perimeter");
                });

            modelBuilder.Entity("HealthDevice.DTO.Spo2", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ElderId")
                        .HasColumnType("text");

                    b.Property<float>("MaxSpO2")
                        .HasColumnType("real");

                    b.Property<float>("MinSpO2")
                        .HasColumnType("real");

                    b.Property<float>("SpO2")
                        .HasColumnType("real");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ElderId");

                    b.ToTable("SpO2");
                });

            modelBuilder.Entity("HealthDevice.DTO.Steps", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ElderId")
                        .HasColumnType("text");

                    b.Property<int>("StepsCount")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ElderId");

                    b.ToTable("Steps");
                });

            modelBuilder.Entity("HealthDevice.DTO.Elder", b =>
                {
                    b.HasOne("HealthDevice.DTO.Caregiver", null)
                        .WithMany("Elders")
                        .HasForeignKey("CaregiverId");

                    b.HasOne("HealthDevice.DTO.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("HealthDevice.DTO.Perimeter", "Perimeter")
                        .WithMany()
                        .HasForeignKey("PerimeterId");

                    b.Navigation("Location");

                    b.Navigation("Perimeter");
                });

            modelBuilder.Entity("HealthDevice.DTO.FallInfo", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("FallInfo")
                        .HasForeignKey("ElderId");

                    b.HasOne("HealthDevice.DTO.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Location");
                });

            modelBuilder.Entity("HealthDevice.DTO.GPS", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("GPSData")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Heartrate", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("Heartrate")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Kilometer", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("Distance")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Max30102", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("MAX30102Data")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Perimeter", b =>
                {
                    b.HasOne("HealthDevice.DTO.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Location");
                });

            modelBuilder.Entity("HealthDevice.DTO.Spo2", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("SpO2")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Steps", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("Steps")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Caregiver", b =>
                {
                    b.Navigation("Elders");
                });

            modelBuilder.Entity("HealthDevice.DTO.Elder", b =>
                {
                    b.Navigation("Distance");

                    b.Navigation("FallInfo");

                    b.Navigation("GPSData");

                    b.Navigation("Heartrate");

                    b.Navigation("MAX30102Data");

                    b.Navigation("SpO2");

                    b.Navigation("Steps");
                });
#pragma warning restore 612, 618
        }
    }
}
