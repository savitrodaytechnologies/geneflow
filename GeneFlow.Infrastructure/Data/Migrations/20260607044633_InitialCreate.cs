using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeneFlow.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    LabId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                });

            migrationBuilder.CreateTable(
                name: "Labs",
                columns: table => new
                {
                    LabId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    LabName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labs", x => x.LabId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SystemRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "User"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "LabUsers",
                columns: table => new
                {
                    LabUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    LabId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabUsers", x => x.LabUserId);
                    table.ForeignKey(
                        name: "FK_LabUsers_Labs_LabId",
                        column: x => x.LabId,
                        principalTable: "Labs",
                        principalColumn: "LabId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    LabId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_Projects_Labs_LabId",
                        column: x => x.LabId,
                        principalTable: "Labs",
                        principalColumn: "LabId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Experiments",
                columns: table => new
                {
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    LabId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExperimentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExperimentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hypothesis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SampleSource = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TreatmentCondition = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InstrumentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReferenceGene = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ControlSampleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PlateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "96-well"),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Visibility = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experiments", x => x.ExperimentId);
                    table.ForeignKey(
                        name: "FK_Experiments_Labs_LabId",
                        column: x => x.LabId,
                        principalTable: "Labs",
                        principalColumn: "LabId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Experiments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Experiments_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectUsers",
                columns: table => new
                {
                    ProjectUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Member"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectUsers", x => x.ProjectUserId);
                    table.ForeignKey(
                        name: "FK_ProjectUsers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisRuns",
                columns: table => new
                {
                    AnalysisRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ControlSampleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReferenceGene = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CalculationMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "DeltaDeltaCt"),
                    HighCtThreshold = table.Column<decimal>(type: "decimal(10,4)", nullable: false, defaultValue: 35m),
                    ReplicateSdThreshold = table.Column<decimal>(type: "decimal(10,4)", nullable: false, defaultValue: 0.5m),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisRuns", x => x.AnalysisRunId);
                    table.ForeignKey(
                        name: "FK_AnalysisRuns_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "ExperimentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalysisRuns_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExperimentNotes",
                columns: table => new
                {
                    ExperimentNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentNotes", x => x.ExperimentNoteId);
                    table.ForeignKey(
                        name: "FK_ExperimentNotes_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "ExperimentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExperimentNotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlateLayouts",
                columns: table => new
                {
                    PlateLayoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "96-well"),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    ColumnCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateLayouts", x => x.PlateLayoutId);
                    table.ForeignKey(
                        name: "FK_PlateLayouts_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "ExperimentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UploadedFiles",
                columns: table => new
                {
                    UploadedFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StoredFilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFiles", x => x.UploadedFileId);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "ExperimentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisResults",
                columns: table => new
                {
                    AnalysisResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    AnalysisRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SampleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TargetGene = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReferenceGene = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SampleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MeanTargetCt = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    SdTargetCt = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    MeanReferenceCt = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    SdReferenceCt = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    DeltaCt = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    DeltaDeltaCt = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    FoldChange = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    Log2FoldChange = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    TargetReplicateCount = table.Column<int>(type: "int", nullable: false),
                    ReferenceReplicateCount = table.Column<int>(type: "int", nullable: false),
                    HasWarning = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisResults", x => x.AnalysisResultId);
                    table.ForeignKey(
                        name: "FK_AnalysisResults_AnalysisRuns_AnalysisRunId",
                        column: x => x.AnalysisRunId,
                        principalTable: "AnalysisRuns",
                        principalColumn: "AnalysisRunId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalysisResults_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "ExperimentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExportFiles",
                columns: table => new
                {
                    ExportFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportFiles", x => x.ExportFileId);
                    table.ForeignKey(
                        name: "FK_ExportFiles_AnalysisRuns_AnalysisRunId",
                        column: x => x.AnalysisRunId,
                        principalTable: "AnalysisRuns",
                        principalColumn: "AnalysisRunId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExportFiles_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "ExperimentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExportFiles_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlateWells",
                columns: table => new
                {
                    PlateWellId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    PlateLayoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WellId = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    RowLabel = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ColumnNumber = table.Column<int>(type: "int", nullable: false),
                    SampleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TargetGene = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceGene = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SampleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReplicateGroup = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CtValue = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    IsExcluded = table.Column<bool>(type: "bit", nullable: false),
                    ExclusionReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateWells", x => x.PlateWellId);
                    table.ForeignKey(
                        name: "FK_PlateWells_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "ExperimentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlateWells_PlateLayouts_PlateLayoutId",
                        column: x => x.PlateLayoutId,
                        principalTable: "PlateLayouts",
                        principalColumn: "PlateLayoutId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RawCtData",
                columns: table => new
                {
                    RawCtDataId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    ExperimentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WellId = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    SampleNameFromFile = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TargetGeneFromFile = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CtValueRaw = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CtValue = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    ValidationMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawCtData", x => x.RawCtDataId);
                    table.ForeignKey(
                        name: "FK_RawCtData_Experiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiments",
                        principalColumn: "ExperimentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawCtData_UploadedFiles_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "UploadedFileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisWarnings",
                columns: table => new
                {
                    AnalysisWarningId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    AnalysisRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlateWellId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WarningType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WarningMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisWarnings", x => x.AnalysisWarningId);
                    table.ForeignKey(
                        name: "FK_AnalysisWarnings_AnalysisResults_AnalysisResultId",
                        column: x => x.AnalysisResultId,
                        principalTable: "AnalysisResults",
                        principalColumn: "AnalysisResultId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalysisWarnings_AnalysisRuns_AnalysisRunId",
                        column: x => x.AnalysisRunId,
                        principalTable: "AnalysisRuns",
                        principalColumn: "AnalysisRunId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisResults_AnalysisRunId",
                table: "AnalysisResults",
                column: "AnalysisRunId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisResults_ExperimentId",
                table: "AnalysisResults",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRuns_CreatedByUserId",
                table: "AnalysisRuns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRuns_ExperimentId",
                table: "AnalysisRuns",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisWarnings_AnalysisResultId",
                table: "AnalysisWarnings",
                column: "AnalysisResultId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisWarnings_AnalysisRunId",
                table: "AnalysisWarnings",
                column: "AnalysisRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentNotes_ExperimentId",
                table: "ExperimentNotes",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentNotes_UserId",
                table: "ExperimentNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiments_LabId",
                table: "Experiments",
                column: "LabId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiments_OwnerUserId",
                table: "Experiments",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Experiments_ProjectId",
                table: "Experiments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportFiles_AnalysisRunId",
                table: "ExportFiles",
                column: "AnalysisRunId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportFiles_CreatedByUserId",
                table: "ExportFiles",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportFiles_ExperimentId",
                table: "ExportFiles",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_LabUsers_LabId_UserId",
                table: "LabUsers",
                columns: new[] { "LabId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabUsers_UserId",
                table: "LabUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateLayouts_ExperimentId",
                table: "PlateLayouts",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateWells_ExperimentId",
                table: "PlateWells",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlateWells_PlateLayoutId_WellId",
                table: "PlateWells",
                columns: new[] { "PlateLayoutId", "WellId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedByUserId",
                table: "Projects",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_LabId",
                table: "Projects",
                column: "LabId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUsers_ProjectId_UserId",
                table: "ProjectUsers",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUsers_UserId",
                table: "ProjectUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RawCtData_ExperimentId",
                table: "RawCtData",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_RawCtData_UploadedFileId",
                table: "RawCtData",
                column: "UploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_ExperimentId",
                table: "UploadedFiles",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_UploadedByUserId",
                table: "UploadedFiles",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisWarnings");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ExperimentNotes");

            migrationBuilder.DropTable(
                name: "ExportFiles");

            migrationBuilder.DropTable(
                name: "LabUsers");

            migrationBuilder.DropTable(
                name: "PlateWells");

            migrationBuilder.DropTable(
                name: "ProjectUsers");

            migrationBuilder.DropTable(
                name: "RawCtData");

            migrationBuilder.DropTable(
                name: "AnalysisResults");

            migrationBuilder.DropTable(
                name: "PlateLayouts");

            migrationBuilder.DropTable(
                name: "UploadedFiles");

            migrationBuilder.DropTable(
                name: "AnalysisRuns");

            migrationBuilder.DropTable(
                name: "Experiments");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Labs");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
