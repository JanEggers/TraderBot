﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TraderBot.Models;

namespace TraderBot.Migrations;

[DbContext(typeof(TradingContext))]
[Migration("20210821020338_Initial")]
partial class Initial
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "5.0.3");

        modelBuilder.Entity("TraderBot.Models.StockDataPoint", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<decimal>("AdjustedClosingPrice")
                    .HasColumnType("TEXT");

                b.Property<decimal>("ClosingPrice")
                    .HasColumnType("TEXT");

                b.Property<decimal>("HighestPrice")
                    .HasColumnType("TEXT");

                b.Property<decimal>("LowestPrice")
                    .HasColumnType("TEXT");

                b.Property<decimal>("OpeningPrice")
                    .HasColumnType("TEXT");

                b.Property<DateTime>("Time")
                    .HasColumnType("TEXT");

                b.Property<int>("TimeSeriesId")
                    .HasColumnType("INTEGER");

                b.Property<long>("Volume")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.HasIndex("TimeSeriesId", "Time")
                    .IsUnique();

                b.ToTable("StockDataPoints");
            });

        modelBuilder.Entity("TraderBot.Models.Symbol", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Name")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("Name")
                    .IsUnique();

                b.ToTable("Symbols");
            });

        modelBuilder.Entity("TraderBot.Models.TimeSeries", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Interval")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<int>("SymbolId")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.HasIndex("SymbolId", "Interval")
                    .IsUnique();

                b.ToTable("TimeSeries");
            });

        modelBuilder.Entity("TraderBot.Models.StockDataPoint", b =>
            {
                b.HasOne("TraderBot.Models.TimeSeries", "TimeSeries")
                    .WithMany("DataPoints")
                    .HasForeignKey("TimeSeriesId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("TimeSeries");
            });

        modelBuilder.Entity("TraderBot.Models.TimeSeries", b =>
            {
                b.HasOne("TraderBot.Models.Symbol", "Symbol")
                    .WithMany("TimeSeries")
                    .HasForeignKey("SymbolId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Symbol");
            });

        modelBuilder.Entity("TraderBot.Models.Symbol", b =>
            {
                b.Navigation("TimeSeries");
            });

        modelBuilder.Entity("TraderBot.Models.TimeSeries", b =>
            {
                b.Navigation("DataPoints");
            });
#pragma warning restore 612, 618
    }
}
