/*
Copyright (c) 2019 Veritas Technologies LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ARP.Database.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locales",
                columns: table => new
                {
                    LocaleCode = table.Column<string>(maxLength: 32, nullable: false),
                    LocaleName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locales", x => x.LocaleCode);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectName = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectName);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserName = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserName);
                });

            migrationBuilder.CreateTable(
                name: "Screens",
                columns: table => new
                {
                    ScreenName = table.Column<string>(maxLength: 260, nullable: false),
                    ProjectName = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screens", x => new { x.ScreenName, x.ProjectName });
                    table.ForeignKey(
                        name: "FK_Screens_Projects_ProjectName",
                        column: x => x.ProjectName,
                        principalTable: "Projects",
                        principalColumn: "ProjectName",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Builds",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    BuildName = table.Column<string>(nullable: false),
                    ProjectName = table.Column<string>(maxLength: 100, nullable: false),
                    Status = table.Column<int>(nullable: false),
                    ModificationTime = table.Column<DateTime>(nullable: false),
                    ModifiedByUser = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Builds", x => x.Id);
                    table.UniqueConstraint("AK_Builds_ProjectName_BuildName", x => new { x.ProjectName, x.BuildName });
                    table.ForeignKey(
                        name: "FK_Builds_Users_ModifiedByUser",
                        column: x => x.ModifiedByUser,
                        principalTable: "Users",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Builds_Projects_ProjectName",
                        column: x => x.ProjectName,
                        principalTable: "Projects",
                        principalColumn: "ProjectName",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    IssueId = table.Column<Guid>(nullable: false),
                    ScreenName = table.Column<string>(maxLength: 260, nullable: true),
                    ProjectName = table.Column<string>(maxLength: 100, nullable: true),
                    LocaleCode = table.Column<string>(maxLength: 32, nullable: true),
                    ModifiedInBuildId = table.Column<Guid>(nullable: false),
                    ModificationTime = table.Column<DateTime>(nullable: false),
                    ModifiedByUser = table.Column<string>(maxLength: 100, nullable: true),
                    IssueType = table.Column<int>(nullable: false),
                    IssueSeverity = table.Column<int>(nullable: false),
                    IssueStatus = table.Column<int>(nullable: false),
                    Identifier = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    X = table.Column<int>(nullable: false),
                    Y = table.Column<int>(nullable: false),
                    Width = table.Column<int>(nullable: false),
                    Height = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.IssueId);
                    table.ForeignKey(
                        name: "FK_Issues_Locales_LocaleCode",
                        column: x => x.LocaleCode,
                        principalTable: "Locales",
                        principalColumn: "LocaleCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Users_ModifiedByUser",
                        column: x => x.ModifiedByUser,
                        principalTable: "Users",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Builds_ModifiedInBuildId",
                        column: x => x.ModifiedInBuildId,
                        principalTable: "Builds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Screens_ScreenName_ProjectName",
                        columns: x => new { x.ScreenName, x.ProjectName },
                        principalTable: "Screens",
                        principalColumns: new[] { "ScreenName", "ProjectName" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScreensInBuilds",
                columns: table => new
                {
                    ScreenInBuildId = table.Column<Guid>(nullable: false),
                    ScreenName = table.Column<string>(maxLength: 260, nullable: false),
                    ProjectName = table.Column<string>(maxLength: 100, nullable: false),
                    BuildId = table.Column<Guid>(nullable: false),
                    LocaleCode = table.Column<string>(maxLength: 32, nullable: false),
                    ScreenUrl = table.Column<string>(nullable: true),
                    ModificationTime = table.Column<DateTime>(nullable: false),
                    ModifiedByUser = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreensInBuilds", x => x.ScreenInBuildId);
                    table.UniqueConstraint("AK_ScreensInBuilds_ProjectName_ScreenName_LocaleCode_BuildId", x => new { x.ProjectName, x.ScreenName, x.LocaleCode, x.BuildId });
                    table.ForeignKey(
                        name: "FK_ScreensInBuilds_Builds_BuildId",
                        column: x => x.BuildId,
                        principalTable: "Builds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreensInBuilds_Locales_LocaleCode",
                        column: x => x.LocaleCode,
                        principalTable: "Locales",
                        principalColumn: "LocaleCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreensInBuilds_Users_ModifiedByUser",
                        column: x => x.ModifiedByUser,
                        principalTable: "Users",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreensInBuilds_Screens_ScreenName_ProjectName",
                        columns: x => new { x.ScreenName, x.ProjectName },
                        principalTable: "Screens",
                        principalColumns: new[] { "ScreenName", "ProjectName" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssueRevisions",
                columns: table => new
                {
                    IssueId = table.Column<Guid>(nullable: false),
                    RevisionNo = table.Column<int>(nullable: false),
                    ScreenName = table.Column<string>(maxLength: 260, nullable: true),
                    ProjectName = table.Column<string>(maxLength: 100, nullable: true),
                    LocaleCode = table.Column<string>(maxLength: 32, nullable: true),
                    ModifiedInBuildId = table.Column<Guid>(nullable: false),
                    ModificationTime = table.Column<DateTime>(nullable: false),
                    ModifiedByUser = table.Column<string>(maxLength: 100, nullable: true),
                    IssueType = table.Column<int>(nullable: false),
                    IssueSeverity = table.Column<int>(nullable: false),
                    IssueStatus = table.Column<int>(nullable: false),
                    Identifier = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    X = table.Column<int>(nullable: false),
                    Y = table.Column<int>(nullable: false),
                    Width = table.Column<int>(nullable: false),
                    Height = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueRevisions", x => new { x.IssueId, x.RevisionNo });
                    table.ForeignKey(
                        name: "FK_IssueRevisions_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "IssueId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comparisons",
                columns: table => new
                {
                    SourceScreenInBuildId = table.Column<Guid>(nullable: false),
                    TargetScreenInBuildId = table.Column<Guid>(nullable: false),
                    Difference = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comparisons", x => new { x.SourceScreenInBuildId, x.TargetScreenInBuildId });
                    table.ForeignKey(
                        name: "FK_Comparisons_ScreensInBuilds_SourceScreenInBuildId",
                        column: x => x.SourceScreenInBuildId,
                        principalTable: "ScreensInBuilds",
                        principalColumn: "ScreenInBuildId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comparisons_ScreensInBuilds_TargetScreenInBuildId",
                        column: x => x.TargetScreenInBuildId,
                        principalTable: "ScreensInBuilds",
                        principalColumn: "ScreenInBuildId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Builds_ModifiedByUser",
                table: "Builds",
                column: "ModifiedByUser");

            migrationBuilder.CreateIndex(
                name: "IX_Comparisons_TargetScreenInBuildId",
                table: "Comparisons",
                column: "TargetScreenInBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_LocaleCode",
                table: "Issues",
                column: "LocaleCode");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ModifiedByUser",
                table: "Issues",
                column: "ModifiedByUser");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ModifiedInBuildId",
                table: "Issues",
                column: "ModifiedInBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ScreenName_ProjectName",
                table: "Issues",
                columns: new[] { "ScreenName", "ProjectName" });

            migrationBuilder.CreateIndex(
                name: "IX_Screens_ProjectName",
                table: "Screens",
                column: "ProjectName");

            migrationBuilder.CreateIndex(
                name: "IX_ScreensInBuilds_BuildId",
                table: "ScreensInBuilds",
                column: "BuildId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreensInBuilds_LocaleCode",
                table: "ScreensInBuilds",
                column: "LocaleCode");

            migrationBuilder.CreateIndex(
                name: "IX_ScreensInBuilds_ModifiedByUser",
                table: "ScreensInBuilds",
                column: "ModifiedByUser");

            migrationBuilder.CreateIndex(
                name: "IX_ScreensInBuilds_ScreenName_ProjectName",
                table: "ScreensInBuilds",
                columns: new[] { "ScreenName", "ProjectName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comparisons");

            migrationBuilder.DropTable(
                name: "IssueRevisions");

            migrationBuilder.DropTable(
                name: "ScreensInBuilds");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "Locales");

            migrationBuilder.DropTable(
                name: "Builds");

            migrationBuilder.DropTable(
                name: "Screens");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
