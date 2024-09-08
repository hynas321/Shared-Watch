﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebApi.Core.Entities.ChatMessage", b =>
                {
                    b.Property<string>("Username")
                        .HasColumnType("text")
                        .HasColumnOrder(0);

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnOrder(1);

                    b.Property<string>("RoomHash")
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Username", "Date");

                    b.HasIndex("RoomHash");

                    b.ToTable("ChatMessages");
                });

            modelBuilder.Entity("WebApi.Core.Entities.PlaylistVideo", b =>
                {
                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.Property<double>("Duration")
                        .HasColumnType("double precision");

                    b.Property<string>("RoomHash")
                        .HasColumnType("text");

                    b.Property<string>("ThumbnailUrl")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.HasKey("Hash");

                    b.HasIndex("RoomHash");

                    b.ToTable("PlaylistVideos");
                });

            modelBuilder.Entity("WebApi.Core.Entities.Room", b =>
                {
                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.Property<List<string>>("AdminTokens")
                        .HasColumnType("text[]");

                    b.HasKey("Hash");

                    b.HasIndex("Hash")
                        .IsUnique();

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("WebApi.Core.Entities.RoomSettings", b =>
                {
                    b.Property<string>("RoomHash")
                        .HasColumnType("text");

                    b.Property<int>("MaxUsers")
                        .HasColumnType("integer");

                    b.Property<string>("RoomName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RoomPassword")
                        .HasColumnType("text");

                    b.Property<int>("RoomType")
                        .HasColumnType("integer");

                    b.HasKey("RoomHash");

                    b.ToTable("RoomSettings");
                });

            modelBuilder.Entity("WebApi.Core.Entities.User", b =>
                {
                    b.Property<string>("AuthorizationToken")
                        .HasColumnType("text");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("boolean");

                    b.Property<string>("RoomHash")
                        .HasColumnType("text");

                    b.Property<string>("SignalRConnectionId")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("AuthorizationToken");

                    b.HasIndex("RoomHash");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("WebApi.Core.Entities.UserPermissions", b =>
                {
                    b.Property<string>("RoomHash")
                        .HasColumnType("text");

                    b.Property<bool>("CanAddChatMessage")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanAddVideo")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanRemoveVideo")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanSkipVideo")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanStartOrPauseVideo")
                        .HasColumnType("boolean");

                    b.HasKey("RoomHash");

                    b.ToTable("UserPermissions");
                });

            modelBuilder.Entity("WebApi.Core.Entities.VideoPlayer", b =>
                {
                    b.Property<string>("RoomHash")
                        .HasColumnType("text");

                    b.Property<double>("CurrentTime")
                        .HasColumnType("double precision");

                    b.Property<bool>("IsPlaying")
                        .HasColumnType("boolean");

                    b.Property<string>("PlaylistVideoHash")
                        .HasColumnType("text");

                    b.Property<bool>("SetPlayedSecondsCalled")
                        .HasColumnType("boolean");

                    b.HasKey("RoomHash");

                    b.HasIndex("PlaylistVideoHash");

                    b.ToTable("VideoPlayers");
                });

            modelBuilder.Entity("WebApi.Core.Entities.ChatMessage", b =>
                {
                    b.HasOne("WebApi.Core.Entities.Room", null)
                        .WithMany("ChatMessages")
                        .HasForeignKey("RoomHash")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApi.Core.Entities.PlaylistVideo", b =>
                {
                    b.HasOne("WebApi.Core.Entities.Room", null)
                        .WithMany("PlaylistVideos")
                        .HasForeignKey("RoomHash")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApi.Core.Entities.RoomSettings", b =>
                {
                    b.HasOne("WebApi.Core.Entities.Room", null)
                        .WithOne("RoomSettings")
                        .HasForeignKey("WebApi.Core.Entities.RoomSettings", "RoomHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebApi.Core.Entities.User", b =>
                {
                    b.HasOne("WebApi.Core.Entities.Room", null)
                        .WithMany("Users")
                        .HasForeignKey("RoomHash")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApi.Core.Entities.UserPermissions", b =>
                {
                    b.HasOne("WebApi.Core.Entities.Room", null)
                        .WithOne("UserPermissions")
                        .HasForeignKey("WebApi.Core.Entities.UserPermissions", "RoomHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebApi.Core.Entities.VideoPlayer", b =>
                {
                    b.HasOne("WebApi.Core.Entities.PlaylistVideo", "PlaylistVideo")
                        .WithMany()
                        .HasForeignKey("PlaylistVideoHash");

                    b.HasOne("WebApi.Core.Entities.Room", null)
                        .WithOne("VideoPlayer")
                        .HasForeignKey("WebApi.Core.Entities.VideoPlayer", "RoomHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlaylistVideo");
                });

            modelBuilder.Entity("WebApi.Core.Entities.Room", b =>
                {
                    b.Navigation("ChatMessages");

                    b.Navigation("PlaylistVideos");

                    b.Navigation("RoomSettings");

                    b.Navigation("UserPermissions");

                    b.Navigation("Users");

                    b.Navigation("VideoPlayer");
                });
#pragma warning restore 612, 618
        }
    }
}
