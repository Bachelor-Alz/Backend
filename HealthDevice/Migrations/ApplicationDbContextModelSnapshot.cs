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

                    b.Property<int>("locationid")
                        .HasColumnType("integer");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("perimeterId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CaregiverId");

                    b.HasIndex("locationid");

                    b.HasIndex("perimeterId");

                    b.ToTable("Elders");
                });

            modelBuilder.Entity("HealthDevice.DTO.FallInfo", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<string>("status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("FallInfos", (string)null);
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

            modelBuilder.Entity("HealthDevice.DTO.MPU6050", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<float>("AccelerationX")
                        .HasColumnType("real");

                    b.Property<float>("AccelerationY")
                        .HasColumnType("real");

                    b.Property<float>("AccelerationZ")
                        .HasColumnType("real");

                    b.Property<float>("GyroscopeX")
                        .HasColumnType("real");

                    b.Property<float>("GyroscopeY")
                        .HasColumnType("real");

                    b.Property<float>("GyroscopeZ")
                        .HasColumnType("real");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("MPU6050Datas", (string)null);
                });

            modelBuilder.Entity("HealthDevice.DTO.Max30102", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<float?>("HeartRate")
                        .HasColumnType("real");

                    b.Property<float?>("SpO2")
                        .HasColumnType("real");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Max30102Datas", (string)null);
                });

            modelBuilder.Entity("HealthDevice.DTO.Neo_6m", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<float>("Course")
                        .HasColumnType("real");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<double>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<char>("LatitudeDirection")
                        .HasColumnType("character(1)");

                    b.Property<double>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<char>("LongitudeDirection")
                        .HasColumnType("character(1)");

                    b.Property<TimeSpan>("UtcTime")
                        .HasColumnType("interval");

                    b.HasKey("Id");

                    b.ToTable("Neo_6mDatas", (string)null);
                });

            modelBuilder.Entity("HealthDevice.DTO.Perimiter", b =>
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

                    b.ToTable("Perimiter");
                });

            modelBuilder.Entity("HealthDevice.DTO.Elder", b =>
                {
                    b.HasOne("HealthDevice.DTO.Caregiver", null)
                        .WithMany("elders")
                        .HasForeignKey("CaregiverId");

                    b.HasOne("HealthDevice.DTO.Location", "location")
                        .WithMany()
                        .HasForeignKey("locationid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("HealthDevice.DTO.Perimiter", "perimeter")
                        .WithMany()
                        .HasForeignKey("perimeterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("location");

                    b.Navigation("perimeter");
                });

            modelBuilder.Entity("HealthDevice.DTO.FallInfo", b =>
                {
                    b.HasOne("HealthDevice.DTO.Location", "location")
                        .WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("location");
                });

            modelBuilder.Entity("HealthDevice.DTO.Heartrate", b =>
                {
                    b.HasOne("HealthDevice.DTO.Elder", null)
                        .WithMany("heartrates")
                        .HasForeignKey("ElderId");
                });

            modelBuilder.Entity("HealthDevice.DTO.Perimiter", b =>
                {
                    b.HasOne("HealthDevice.DTO.Location", "location")
                        .WithMany()
                        .HasForeignKey("locationid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("location");
                });

            modelBuilder.Entity("HealthDevice.DTO.Caregiver", b =>
                {
                    b.Navigation("elders");
                });

            modelBuilder.Entity("HealthDevice.DTO.Elder", b =>
                {
                    b.Navigation("heartrates");
                });
#pragma warning restore 612, 618
        }
    }
}
