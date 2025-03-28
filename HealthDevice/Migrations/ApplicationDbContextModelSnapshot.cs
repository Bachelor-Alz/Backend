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

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Caregivers");
                });

            modelBuilder.Entity("HealthDevice.DTO.Elder", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("CaregiverId")
                        .HasColumnType("text");

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

                    b.Property<string>("arduino")
                        .HasColumnType("text");

                    b.Property<int?>("fallInfoId")
                        .HasColumnType("integer");

                    b.Property<int>("locationid")
                        .HasColumnType("integer");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("perimeterId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CaregiverId");

                    b.HasIndex("fallInfoId");

                    b.HasIndex("locationid");

                    b.HasIndex("perimeterId");

                    b.ToTable("Elders");
                });

            modelBuilder.Entity("HealthDevice.DTO.FallInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("locationid")
                        .HasColumnType("integer");

                    b.Property<DateTime>("timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("locationid");

                    b.ToTable("FallInfos");
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

                    b.ToTable("GPS_Data", (string)null);
                });

            modelBuilder.Entity("HealthDevice.DTO.Heartrate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AvgRate")
                        .HasColumnType("integer");

                    b.Property<string>("ElderId")
                        .HasColumnType("text");

                    b.Property<int>("MaxRate")
                        .HasColumnType("integer");

                    b.Property<int>("MinRate")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ElderId");

                    b.ToTable("Heartrates");
                });

            modelBuilder.Entity("HealthDevice.DTO.Location", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<int>("latitude")
                        .HasColumnType("integer");

                    b.Property<int>("longitude")
                        .HasColumnType("integer");

                    b.Property<DateTime>("timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("id");

                    b.ToTable("Locations", (string)null);
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

                    b.Property<int>("HeartRate")
                        .HasColumnType("integer");

                    b.Property<float>("SpO2")
                        .HasColumnType("real");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("ElderId");

                    b.ToTable("Max30102Data", (string)null);
                });

            modelBuilder.Entity("HealthDevice.DTO.Perimeter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("locationid")
                        .HasColumnType("integer");

                    b.Property<int>("radius")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("locationid");

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

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<float>("spO2")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("ElderId");

                    b.ToTable("SpO2s");
                });

            modelBuilder.Entity("HealthDevice.DTO.Elder", b =>
                {
                    b.HasOne("HealthDevice.DTO.Caregiver", null)
                        .WithMany("elders")
                        .HasForeignKey("CaregiverId");

                    b.HasOne("HealthDevice.DTO.FallInfo", "fallInfo")
                        .WithMany()
                        .HasForeignKey("fallInfoId");

                    b.HasOne("HealthDevice.DTO.Location", "location")
                        .WithMany()
                        .HasForeignKey("locationid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("HealthDevice.DTO.Perimeter", "perimeter")
                        .WithMany()
                        .HasForeignKey("perimeterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("fallInfo");

                    b.Navigation("location");

                    b.Navigation("perimeter");
                });

            modelBuilder.Entity("HealthDevice.DTO.FallInfo", b =>
                {
                    b.HasOne("HealthDevice.DTO.Location", "location")
                        .WithMany()
                        .HasForeignKey("locationid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("location");
                });

            modelBuilder.Entity("HealthDevice.DTO.GPS", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("gpsData")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Heartrate", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("heartRates")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Max30102", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("Max30102Datas")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Perimeter", b =>
                {
                    b.HasOne("HealthDevice.DTO.Location", "location")
                        .WithMany()
                        .HasForeignKey("locationid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("location");
                });

            modelBuilder.Entity("HealthDevice.DTO.Spo2", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("spo2s")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Caregiver", b =>
                {
                    b.Navigation("elders");
                });

            modelBuilder.Entity("HealthDevice.DTO.Elder", b =>
                {
                    b.Navigation("Max30102Datas");

                    b.Navigation("gpsData");

                    b.Navigation("heartRates");

                    b.Navigation("spo2s");
                });
#pragma warning restore 612, 618
        }
    }
}
