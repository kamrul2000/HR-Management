using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DutySlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlotName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    BreakDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    LateToleranceMinutes = table.Column<int>(type: "int", nullable: false),
                    TotalWorkingHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsNightShift = table.Column<bool>(type: "bit", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DutySlots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    IsCarryForward = table.Column<bool>(type: "bit", nullable: false),
                    MaxCarryForwardDays = table.Column<int>(type: "int", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    RequiresDocument = table.Column<bool>(type: "bit", nullable: false),
                    MinNoticeDays = table.Column<int>(type: "int", nullable: false),
                    MaxConsecutiveDays = table.Column<int>(type: "int", nullable: true),
                    GenderRestriction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalaryHeads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeadName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HeadCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HeadType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CalculationMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(6,4)", nullable: true),
                    BaseHeadId = table.Column<int>(type: "int", nullable: true),
                    IsFixed = table.Column<bool>(type: "bit", nullable: false),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false),
                    IsProvidentFundApplicable = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryHeads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryHeads_SalaryHeads_BaseHeadId",
                        column: x => x.BaseHeadId,
                        principalTable: "SalaryHeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxSlabConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYear = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    TaxFreeThreshold = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxSlabConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ManagerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxSlabs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxSlabConfigId = table.Column<int>(type: "int", nullable: false),
                    SlabOrder = table.Column<int>(type: "int", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxSlabs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxSlabs_TaxSlabConfigs_TaxSlabConfigId",
                        column: x => x.TaxSlabConfigId,
                        principalTable: "TaxSlabConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HolidayCalendars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HolidayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HolidayDate = table.Column<DateTime>(type: "date", nullable: false),
                    HolidayType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsRecurringYearly = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayCalendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HolidayCalendars_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OffDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    DayName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OffDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OffDays_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Designations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Grade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Designations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaritalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    DesignationId = table.Column<int>(type: "int", nullable: false),
                    EmploymentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalTable: "Designations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    DutySlotId = table.Column<int>(type: "int", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "date", nullable: false),
                    PunchInTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    PunchOutTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsLate = table.Column<bool>(type: "bit", nullable: false),
                    LateMinutes = table.Column<int>(type: "int", nullable: false),
                    ActualWorkingMinutes = table.Column<int>(type: "int", nullable: false),
                    ScheduledWorkingMinutes = table.Column<int>(type: "int", nullable: false),
                    OvertimeMinutes = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_DutySlots_DutySlotId",
                        column: x => x.DutySlotId,
                        principalTable: "DutySlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveAllotments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    AllocatedDays = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    UsedDays = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    CarriedForwardDays = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveAllotments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveAllotments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveAllotments_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    RequestedTenureMonths = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RecommendedById = table.Column<int>(type: "int", nullable: true),
                    RecommendationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecommendationRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectedById = table.Column<int>(type: "int", nullable: true),
                    RejectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanApplications_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryStructures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "date", nullable: true),
                    BasicSalary = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryStructures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryStructures_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Overtimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    AttendanceId = table.Column<int>(type: "int", nullable: false),
                    OvertimeDate = table.Column<DateTime>(type: "date", nullable: false),
                    RequestedMinutes = table.Column<int>(type: "int", nullable: false),
                    ApprovedMinutes = table.Column<int>(type: "int", nullable: false),
                    OvertimeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Overtimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Overtimes_Attendances_AttendanceId",
                        column: x => x.AttendanceId,
                        principalTable: "Attendances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Overtimes_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "int", nullable: false),
                    LeaveAllotmentId = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<DateTime>(type: "date", nullable: false),
                    ToDate = table.Column<DateTime>(type: "date", nullable: false),
                    TotalDays = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveApplications_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveApplications_LeaveAllotments_LeaveAllotmentId",
                        column: x => x.LeaveAllotmentId,
                        principalTable: "LeaveAllotments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveApplications_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLoans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanApplicationId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LoanNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    InterestType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TenureMonths = table.Column<int>(type: "int", nullable: false),
                    MonthlyInstallment = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TotalRepayable = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    DisbursementDate = table.Column<DateTime>(type: "date", nullable: false),
                    FirstInstallmentMonth = table.Column<int>(type: "int", nullable: false),
                    FirstInstallmentYear = table.Column<int>(type: "int", nullable: false),
                    TotalPaid = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PaidInstallments = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLoans_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeLoans_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanApplicationId = table.Column<int>(type: "int", nullable: false),
                    ApprovedById = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    ApprovedTenureMonths = table.Column<int>(type: "int", nullable: true),
                    MonthlyInstallment = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    InterestRate = table.Column<decimal>(type: "decimal(6,4)", nullable: true),
                    InterestType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanApprovals_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanRecommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanApplicationId = table.Column<int>(type: "int", nullable: false),
                    RecommendedById = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecommendedAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    RecommendedTenureMonths = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanRecommendations_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryCalculations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SalaryStructureId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    TotalWorkingDays = table.Column<int>(type: "int", nullable: false),
                    PresentDays = table.Column<int>(type: "int", nullable: false),
                    AbsentDays = table.Column<int>(type: "int", nullable: false),
                    HalfDays = table.Column<int>(type: "int", nullable: false),
                    UnpaidLeaveDays = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    LateDeductionDays = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    OvertimeMinutes = table.Column<int>(type: "int", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    GrossSalary = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TotalEarnings = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    OvertimePay = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    BonusAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    LoanDeduction = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TaxDeduction = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CalculationDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryCalculations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryCalculations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryCalculations_SalaryStructures_SalaryStructureId",
                        column: x => x.SalaryStructureId,
                        principalTable: "SalaryStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryStructureItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryStructureId = table.Column<int>(type: "int", nullable: false),
                    SalaryHeadId = table.Column<int>(type: "int", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    OverridePercentage = table.Column<decimal>(type: "decimal(6,4)", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryStructureItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryStructureItems_SalaryHeads_SalaryHeadId",
                        column: x => x.SalaryHeadId,
                        principalTable: "SalaryHeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryStructureItems_SalaryStructures_SalaryStructureId",
                        column: x => x.SalaryStructureId,
                        principalTable: "SalaryStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BonusCalculations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    BonusType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BonusTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CalculationBasis = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BasisPercentage = table.Column<decimal>(type: "decimal(6,4)", nullable: true),
                    BasicSalarySnapshot = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    GrossSalarySnapshot = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ComputedAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    FinalAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    DisbursementMonth = table.Column<int>(type: "int", nullable: false),
                    DisbursementYear = table.Column<int>(type: "int", nullable: false),
                    IsDisbursedWithSalary = table.Column<bool>(type: "bit", nullable: false),
                    SalaryCalculationId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusCalculations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusCalculations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BonusCalculations_SalaryCalculations_SalaryCalculationId",
                        column: x => x.SalaryCalculationId,
                        principalTable: "SalaryCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanInstallments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeLoanId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    InstallmentNo = table.Column<int>(type: "int", nullable: false),
                    DueMonth = table.Column<int>(type: "int", nullable: false),
                    DueYear = table.Column<int>(type: "int", nullable: false),
                    InstallmentAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SalaryCalculationId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanInstallments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanInstallments_EmployeeLoans_EmployeeLoanId",
                        column: x => x.EmployeeLoanId,
                        principalTable: "EmployeeLoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanInstallments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanInstallments_SalaryCalculations_SalaryCalculationId",
                        column: x => x.SalaryCalculationId,
                        principalTable: "SalaryCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryCalculationDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryCalculationId = table.Column<int>(type: "int", nullable: false),
                    SalaryHeadId = table.Column<int>(type: "int", nullable: false),
                    HeadName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HeadCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HeadType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CalculationMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BaseAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    AppliedPercentage = table.Column<decimal>(type: "decimal(6,4)", nullable: true),
                    ComputedAmount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryCalculationDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryCalculationDetails_SalaryCalculations_SalaryCalculationId",
                        column: x => x.SalaryCalculationId,
                        principalTable: "SalaryCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_AttendanceDate",
                table: "Attendances",
                column: "AttendanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_DutySlotId",
                table: "Attendances",
                column: "DutySlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId",
                table: "Attendances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId_AttendanceDate_SubscriptionId",
                table: "Attendances",
                columns: new[] { "EmployeeId", "AttendanceDate", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_SubscriptionId",
                table: "Attendances",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusCalculations_DisbursementYear_DisbursementMonth",
                table: "BonusCalculations",
                columns: new[] { "DisbursementYear", "DisbursementMonth" });

            migrationBuilder.CreateIndex(
                name: "IX_BonusCalculations_EmployeeId",
                table: "BonusCalculations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusCalculations_SalaryCalculationId",
                table: "BonusCalculations",
                column: "SalaryCalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusCalculations_Status",
                table: "BonusCalculations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BonusCalculations_SubscriptionId",
                table: "BonusCalculations",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyId",
                table: "Branches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyId_Code",
                table: "Branches",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_SubscriptionId",
                table: "Branches",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_SubscriptionId",
                table: "Companies",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_BranchId",
                table: "Departments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_SubscriptionId",
                table: "Departments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Designations_DepartmentId",
                table: "Designations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Designations_SubscriptionId",
                table: "Designations",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DutySlots_SlotName_SubscriptionId",
                table: "DutySlots",
                columns: new[] { "SlotName", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DutySlots_SubscriptionId",
                table: "DutySlots",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLoans_EmployeeId",
                table: "EmployeeLoans",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLoans_LoanApplicationId_SubscriptionId",
                table: "EmployeeLoans",
                columns: new[] { "LoanApplicationId", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLoans_LoanNo_SubscriptionId",
                table: "EmployeeLoans",
                columns: new[] { "LoanNo", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLoans_Status",
                table: "EmployeeLoans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLoans_SubscriptionId",
                table: "EmployeeLoans",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BranchId",
                table: "Employees",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DesignationId",
                table: "Employees",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email_SubscriptionId",
                table: "Employees",
                columns: new[] { "Email", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode_SubscriptionId",
                table: "Employees",
                columns: new[] { "EmployeeCode", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_SubscriptionId",
                table: "Employees",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_HolidayCalendars_BranchId",
                table: "HolidayCalendars",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_HolidayCalendars_HolidayDate",
                table: "HolidayCalendars",
                column: "HolidayDate");

            migrationBuilder.CreateIndex(
                name: "IX_HolidayCalendars_SubscriptionId",
                table: "HolidayCalendars",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveAllotments_EmployeeId",
                table: "LeaveAllotments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveAllotments_EmployeeId_LeaveTypeId_Year_SubscriptionId",
                table: "LeaveAllotments",
                columns: new[] { "EmployeeId", "LeaveTypeId", "Year", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveAllotments_LeaveTypeId",
                table: "LeaveAllotments",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveAllotments_SubscriptionId",
                table: "LeaveAllotments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_ApplicationNo_SubscriptionId",
                table: "LeaveApplications",
                columns: new[] { "ApplicationNo", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_EmployeeId",
                table: "LeaveApplications",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_LeaveAllotmentId",
                table: "LeaveApplications",
                column: "LeaveAllotmentId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_LeaveTypeId",
                table: "LeaveApplications",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_Status",
                table: "LeaveApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_SubscriptionId",
                table: "LeaveApplications",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_Code_SubscriptionId",
                table: "LeaveTypes",
                columns: new[] { "Code", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_Name_SubscriptionId",
                table: "LeaveTypes",
                columns: new[] { "Name", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_SubscriptionId",
                table: "LeaveTypes",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_ApplicationNo_SubscriptionId",
                table: "LoanApplications",
                columns: new[] { "ApplicationNo", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_EmployeeId",
                table: "LoanApplications",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_Status",
                table: "LoanApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_SubscriptionId",
                table: "LoanApplications",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApprovals_LoanApplicationId",
                table: "LoanApprovals",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApprovals_LoanApplicationId_SubscriptionId",
                table: "LoanApprovals",
                columns: new[] { "LoanApplicationId", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanApprovals_SubscriptionId",
                table: "LoanApprovals",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanInstallments_EmployeeId",
                table: "LoanInstallments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanInstallments_EmployeeId_DueYear_DueMonth_Status",
                table: "LoanInstallments",
                columns: new[] { "EmployeeId", "DueYear", "DueMonth", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LoanInstallments_EmployeeLoanId",
                table: "LoanInstallments",
                column: "EmployeeLoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanInstallments_SalaryCalculationId",
                table: "LoanInstallments",
                column: "SalaryCalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanInstallments_Status",
                table: "LoanInstallments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LoanInstallments_SubscriptionId",
                table: "LoanInstallments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRecommendations_LoanApplicationId",
                table: "LoanRecommendations",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRecommendations_LoanApplicationId_SubscriptionId",
                table: "LoanRecommendations",
                columns: new[] { "LoanApplicationId", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanRecommendations_SubscriptionId",
                table: "LoanRecommendations",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_OffDays_BranchId",
                table: "OffDays",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_OffDays_DayOfWeek",
                table: "OffDays",
                column: "DayOfWeek");

            migrationBuilder.CreateIndex(
                name: "IX_OffDays_SubscriptionId",
                table: "OffDays",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Overtimes_AttendanceId",
                table: "Overtimes",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Overtimes_AttendanceId_SubscriptionId",
                table: "Overtimes",
                columns: new[] { "AttendanceId", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Overtimes_EmployeeId",
                table: "Overtimes",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Overtimes_Status",
                table: "Overtimes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Overtimes_SubscriptionId",
                table: "Overtimes",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryCalculationDetails_SalaryCalculationId",
                table: "SalaryCalculationDetails",
                column: "SalaryCalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryCalculationDetails_SalaryHeadId",
                table: "SalaryCalculationDetails",
                column: "SalaryHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryCalculations_EmployeeId",
                table: "SalaryCalculations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryCalculations_EmployeeId_Year_Month_SubscriptionId_Status",
                table: "SalaryCalculations",
                columns: new[] { "EmployeeId", "Year", "Month", "SubscriptionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryCalculations_SalaryStructureId",
                table: "SalaryCalculations",
                column: "SalaryStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryCalculations_Status",
                table: "SalaryCalculations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryCalculations_SubscriptionId",
                table: "SalaryCalculations",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryHeads_BaseHeadId",
                table: "SalaryHeads",
                column: "BaseHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryHeads_HeadCode_SubscriptionId",
                table: "SalaryHeads",
                columns: new[] { "HeadCode", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryHeads_HeadName_SubscriptionId",
                table: "SalaryHeads",
                columns: new[] { "HeadName", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryHeads_HeadType",
                table: "SalaryHeads",
                column: "HeadType");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryHeads_SubscriptionId",
                table: "SalaryHeads",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructureItems_SalaryHeadId",
                table: "SalaryStructureItems",
                column: "SalaryHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructureItems_SalaryStructureId",
                table: "SalaryStructureItems",
                column: "SalaryStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructureItems_SalaryStructureId_SalaryHeadId",
                table: "SalaryStructureItems",
                columns: new[] { "SalaryStructureId", "SalaryHeadId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructures_EmployeeId",
                table: "SalaryStructures",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructures_EmployeeId_IsActive",
                table: "SalaryStructures",
                columns: new[] { "EmployeeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructures_SubscriptionId",
                table: "SalaryStructures",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxSlabConfigs_FiscalYear_SubscriptionId",
                table: "TaxSlabConfigs",
                columns: new[] { "FiscalYear", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxSlabConfigs_SubscriptionId",
                table: "TaxSlabConfigs",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxSlabs_TaxSlabConfigId",
                table: "TaxSlabs",
                column: "TaxSlabConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SubscriptionId",
                table: "Users",
                column: "SubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BonusCalculations");

            migrationBuilder.DropTable(
                name: "HolidayCalendars");

            migrationBuilder.DropTable(
                name: "LeaveApplications");

            migrationBuilder.DropTable(
                name: "LoanApprovals");

            migrationBuilder.DropTable(
                name: "LoanInstallments");

            migrationBuilder.DropTable(
                name: "LoanRecommendations");

            migrationBuilder.DropTable(
                name: "OffDays");

            migrationBuilder.DropTable(
                name: "Overtimes");

            migrationBuilder.DropTable(
                name: "SalaryCalculationDetails");

            migrationBuilder.DropTable(
                name: "SalaryStructureItems");

            migrationBuilder.DropTable(
                name: "TaxSlabs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LeaveAllotments");

            migrationBuilder.DropTable(
                name: "EmployeeLoans");

            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "SalaryCalculations");

            migrationBuilder.DropTable(
                name: "SalaryHeads");

            migrationBuilder.DropTable(
                name: "TaxSlabConfigs");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropTable(
                name: "LoanApplications");

            migrationBuilder.DropTable(
                name: "DutySlots");

            migrationBuilder.DropTable(
                name: "SalaryStructures");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Designations");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
