﻿// <auto-generated />
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240902171116_Migration_3")]
    partial class Migration_3
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DotnetServer.Core.Entities.ChatMessage", b =>
                {
                    b.Property<string>("Username")
                        .HasColumnType("text")
                        .HasColumnOrder(0);

                    b.Property<string>("Date")
                        .HasColumnType("text")
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

            modelBuilder.Entity("DotnetServer.Core.Entities.PlaylistVideo", b =>
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

            modelBuilder.Entity("DotnetServer.Core.Entities.Room", b =>
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

            modelBuilder.Entity("DotnetServer.Core.Entities.RoomSettings", b =>
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

            modelBuilder.Entity("DotnetServer.Core.Entities.User", b =>
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

            modelBuilder.Entity("DotnetServer.Core.Entities.UserPermissions", b =>
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

            modelBuilder.Entity("DotnetServer.Core.Entities.VideoPlayer", b =>
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

            modelBuilder.Entity("DotnetServer.Core.Entities.ChatMessage", b =>
                {
                    b.HasOne("DotnetServer.Core.Entities.Room", "Room")
                        .WithMany("ChatMessages")
                        .HasForeignKey("RoomHash")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Room");
                });

            modelBuilder.Entity("DotnetServer.Core.Entities.PlaylistVideo", b =>
                {
                    b.HasOne("DotnetServer.Core.Entities.Room", "Room")
                        .WithMany("PlaylistVideos")
                        .HasForeignKey("RoomHash")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Room");
                });

            modelBuilder.Entity("DotnetServer.Core.Entities.RoomSettings", b =>
                {
                    b.HasOne("DotnetServer.Core.Entities.Room", "Room")
                        .WithOne("RoomSettings")
                        .HasForeignKey("DotnetServer.Core.Entities.RoomSettings", "RoomHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");
                });

            modelBuilder.Entity("DotnetServer.Core.Entities.User", b =>
                {
                    b.HasOne("DotnetServer.Core.Entities.Room", "Room")
                        .WithMany("Users")
                        .HasForeignKey("RoomHash")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Room");
                });

            modelBuilder.Entity("DotnetServer.Core.Entities.UserPermissions", b =>
                {
                    b.HasOne("DotnetServer.Core.Entities.Room", "Room")
                        .WithOne("UserPermissions")
                        .HasForeignKey("DotnetServer.Core.Entities.UserPermissions", "RoomHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");
                });

            modelBuilder.Entity("DotnetServer.Core.Entities.VideoPlayer", b =>
                {
                    b.HasOne("DotnetServer.Core.Entities.PlaylistVideo", "PlaylistVideo")
                        .WithMany()
                        .HasForeignKey("PlaylistVideoHash");

                    b.HasOne("DotnetServer.Core.Entities.Room", "Room")
                        .WithOne("VideoPlayer")
                        .HasForeignKey("DotnetServer.Core.Entities.VideoPlayer", "RoomHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlaylistVideo");

                    b.Navigation("Room");
                });

            modelBuilder.Entity("DotnetServer.Core.Entities.Room", b =>
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