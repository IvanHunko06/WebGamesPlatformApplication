﻿// <auto-generated />
using System;
using MatchHistoryService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MatchHistoryService.Migrations
{
    [DbContext(typeof(MatchHistoryDbContext))]
    [Migration("20241026124304_first")]
    partial class first
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MatchHistoryService.Models.MatchInformation", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                b.Property<string>("FinishReason")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("GameId")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTime>("TimeBegin")
                    .HasColumnType("datetime2");

                b.Property<DateTime>("TimeEnd")
                    .HasColumnType("datetime2");

                b.HasKey("Id");

                b.ToTable("MatchInformations");
            });

            modelBuilder.Entity("MatchHistoryService.Models.Score", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                b.Property<bool>("IsWinner")
                    .HasColumnType("bit");

                b.Property<int>("MatchInfoId")
                    .HasColumnType("int");

                b.Property<int>("ScorePoints")
                    .HasColumnType("int");

                b.Property<string>("UserId")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.HasIndex("MatchInfoId");

                b.ToTable("PlayerScores");
            });

            modelBuilder.Entity("MatchHistoryService.Models.Score", b =>
            {
                b.HasOne("MatchHistoryService.Models.MatchInformation", "MatchInfo")
                    .WithMany("MatchMembers")
                    .HasForeignKey("MatchInfoId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("MatchInfo");
            });

            modelBuilder.Entity("MatchHistoryService.Models.MatchInformation", b =>
            {
                b.Navigation("MatchMembers");
            });
#pragma warning restore 612, 618
        }
    }
}