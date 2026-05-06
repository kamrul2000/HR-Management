using HRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveApplication> LeaveApplications => Set<LeaveApplication>();
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<SalaryStructureItem> SalaryStructureItems => Set<SalaryStructureItem>();
    public DbSet<DutySlot> DutySlots => Set<DutySlot>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveAllotment> LeaveAllotments => Set<LeaveAllotment>();
    public DbSet<HolidayCalendar> HolidayCalendars => Set<HolidayCalendar>();
    public DbSet<OffDay> OffDays => Set<OffDay>();
    public DbSet<Overtime> Overtimes => Set<Overtime>();
    public DbSet<SalaryHead> SalaryHeads => Set<SalaryHead>();
    public DbSet<SalaryCalculation> SalaryCalculations => Set<SalaryCalculation>();
    public DbSet<SalaryCalculationDetail> SalaryCalculationDetails => Set<SalaryCalculationDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(u => u.PasswordHash)
                .IsRequired();

            entity.Property(u => u.SubscriptionId)
                .IsRequired();

            entity.Property(u => u.CreatedAt).IsRequired();
            entity.Property(u => u.UpdatedAt).IsRequired();

            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.SubscriptionId);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.LogoPath).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.SubscriptionId);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ManagerName).HasMaxLength(100);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Company)
                .WithMany(c => c.Branches)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Branch)
                .WithMany(b => b.Departments)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.BranchId);
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.ToTable("Designations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Grade).HasMaxLength(50);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Designations)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.DepartmentId);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(30);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(80);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(80);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(160);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.Gender).IsRequired().HasMaxLength(20);
            entity.Property(e => e.MaritalStatus).IsRequired().HasMaxLength(20);
            entity.Property(e => e.NationalId).HasMaxLength(50);
            entity.Property(e => e.JoiningDate).IsRequired();
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PhotoPath).HasMaxLength(500);
            entity.Property(e => e.EmploymentType).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => new { e.EmployeeCode, e.SubscriptionId }).IsUnique();
            entity.HasIndex(e => new { e.Email, e.SubscriptionId }).IsUnique();
            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.BranchId);
            entity.HasIndex(e => e.DepartmentId);
            entity.HasIndex(e => e.DesignationId);

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Designation)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.ToTable("Attendances");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.DutySlotId).IsRequired();
            entity.Property(e => e.AttendanceDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
            entity.Property(e => e.IsLate).IsRequired();
            entity.Property(e => e.LateMinutes).IsRequired();
            entity.Property(e => e.ActualWorkingMinutes).IsRequired();
            entity.Property(e => e.ScheduledWorkingMinutes).IsRequired();
            entity.Property(e => e.OvertimeMinutes).IsRequired();
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DutySlot)
                .WithMany()
                .HasForeignKey(e => e.DutySlotId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.DutySlotId);
            entity.HasIndex(e => e.AttendanceDate);
            entity.HasIndex(e => new { e.EmployeeId, e.AttendanceDate, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<LeaveApplication>(entity =>
        {
            entity.ToTable("LeaveApplications");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ApplicationNo).IsRequired().HasMaxLength(30);
            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.LeaveTypeId).IsRequired();
            entity.Property(e => e.LeaveAllotmentId).IsRequired();
            entity.Property(e => e.FromDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.ToDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.TotalDays).HasColumnType("decimal(6,2)").IsRequired();
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.AttachmentPath).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
            entity.Property(e => e.ApprovalRemarks).HasMaxLength(500);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.LeaveType)
                .WithMany(t => t.LeaveApplications)
                .HasForeignKey(e => e.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.LeaveAllotment)
                .WithMany()
                .HasForeignKey(e => e.LeaveAllotmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.LeaveTypeId);
            entity.HasIndex(e => e.LeaveAllotmentId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.ApplicationNo, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<SalaryStructure>(entity =>
        {
            entity.ToTable("SalaryStructures");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.EffectiveFrom).HasColumnType("date").IsRequired();
            entity.Property(e => e.EffectiveTo).HasColumnType("date");
            entity.Property(e => e.BasicSalary).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => new { e.EmployeeId, e.IsActive });
        });

        modelBuilder.Entity<SalaryStructureItem>(entity =>
        {
            entity.ToTable("SalaryStructureItems");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SalaryStructureId).IsRequired();
            entity.Property(e => e.SalaryHeadId).IsRequired();
            entity.Property(e => e.FixedAmount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.OverridePercentage).HasColumnType("decimal(6,4)");
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.SalaryStructure)
                .WithMany(s => s.Items)
                .HasForeignKey(e => e.SalaryStructureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SalaryHead)
                .WithMany()
                .HasForeignKey(e => e.SalaryHeadId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SalaryStructureId);
            entity.HasIndex(e => e.SalaryHeadId);
            entity.HasIndex(e => new { e.SalaryStructureId, e.SalaryHeadId }).IsUnique();
        });

        modelBuilder.Entity<DutySlot>(entity =>
        {
            entity.ToTable("DutySlots");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SlotName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
            entity.Property(e => e.BreakDurationMinutes).IsRequired();
            entity.Property(e => e.LateToleranceMinutes).IsRequired();
            entity.Property(e => e.TotalWorkingHours).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(e => e.IsNightShift).IsRequired();
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => new { e.SlotName, e.SubscriptionId }).IsUnique();
            entity.HasIndex(e => e.SubscriptionId);
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.ToTable("LeaveTypes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsPaid).IsRequired();
            entity.Property(e => e.IsCarryForward).IsRequired();
            entity.Property(e => e.MaxCarryForwardDays).IsRequired();
            entity.Property(e => e.RequiresApproval).IsRequired();
            entity.Property(e => e.RequiresDocument).IsRequired();
            entity.Property(e => e.MinNoticeDays).IsRequired();
            entity.Property(e => e.GenderRestriction).HasMaxLength(20);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => new { e.Name, e.SubscriptionId }).IsUnique();
            entity.HasIndex(e => new { e.Code, e.SubscriptionId }).IsUnique();
            entity.HasIndex(e => e.SubscriptionId);
        });

        modelBuilder.Entity<LeaveAllotment>(entity =>
        {
            entity.ToTable("LeaveAllotments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.LeaveTypeId).IsRequired();
            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.AllocatedDays).HasColumnType("decimal(6,2)").IsRequired();
            entity.Property(e => e.UsedDays).HasColumnType("decimal(6,2)").IsRequired();
            entity.Property(e => e.CarriedForwardDays).HasColumnType("decimal(6,2)").IsRequired();
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.Ignore(e => e.RemainingDays);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.LeaveType)
                .WithMany(t => t.LeaveAllotments)
                .HasForeignKey(e => e.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId, e.Year, e.SubscriptionId }).IsUnique();
            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.LeaveTypeId);
            entity.HasIndex(e => e.EmployeeId);
        });

        modelBuilder.Entity<HolidayCalendar>(entity =>
        {
            entity.ToTable("HolidayCalendars");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.HolidayName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.HolidayDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.HolidayType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsRecurringYearly).IsRequired();
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.HolidayDate);
            entity.HasIndex(e => e.BranchId);
        });

        modelBuilder.Entity<OffDay>(entity =>
        {
            entity.ToTable("OffDays");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DayOfWeek).IsRequired();
            entity.Property(e => e.DayName).IsRequired().HasMaxLength(20);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.BranchId);
            entity.HasIndex(e => e.DayOfWeek);
        });

        modelBuilder.Entity<Overtime>(entity =>
        {
            entity.ToTable("Overtimes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.AttendanceId).IsRequired();
            entity.Property(e => e.OvertimeDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.RequestedMinutes).IsRequired();
            entity.Property(e => e.ApprovedMinutes).IsRequired();
            entity.Property(e => e.OvertimeType).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
            entity.Property(e => e.ApprovalRemarks).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Attendance)
                .WithMany()
                .HasForeignKey(e => e.AttendanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.AttendanceId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.AttendanceId, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<SalaryHead>(entity =>
        {
            entity.ToTable("SalaryHeads");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.HeadName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.HeadCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.HeadType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CalculationMethod).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Percentage).HasColumnType("decimal(6,4)");
            entity.Property(e => e.IsFixed).IsRequired();
            entity.Property(e => e.IsTaxable).IsRequired();
            entity.Property(e => e.IsProvidentFundApplicable).IsRequired();
            entity.Property(e => e.DisplayOrder).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => new { e.HeadName, e.SubscriptionId }).IsUnique();
            entity.HasIndex(e => new { e.HeadCode, e.SubscriptionId }).IsUnique();
            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.HeadType);

            entity.HasOne(e => e.BaseHead)
                .WithMany(e => e.DependentHeads)
                .HasForeignKey(e => e.BaseHeadId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SalaryCalculation>(entity =>
        {
            entity.ToTable("SalaryCalculations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.SalaryStructureId).IsRequired();
            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.Month).IsRequired();
            entity.Property(e => e.TotalWorkingDays).IsRequired();
            entity.Property(e => e.PresentDays).IsRequired();
            entity.Property(e => e.AbsentDays).IsRequired();
            entity.Property(e => e.HalfDays).IsRequired();
            entity.Property(e => e.UnpaidLeaveDays).HasColumnType("decimal(6,2)").IsRequired();
            entity.Property(e => e.LateDeductionDays).HasColumnType("decimal(6,2)").IsRequired();
            entity.Property(e => e.OvertimeMinutes).IsRequired();
            entity.Property(e => e.BasicSalary).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.GrossSalary).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.TotalEarnings).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.TotalDeductions).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.OvertimePay).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.BonusAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.LoanDeduction).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.TaxDeduction).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.NetSalary).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SalaryStructure)
                .WithMany()
                .HasForeignKey(e => e.SalaryStructureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.EmployeeId, e.Year, e.Month, e.SubscriptionId, e.Status });
        });

        modelBuilder.Entity<SalaryCalculationDetail>(entity =>
        {
            entity.ToTable("SalaryCalculationDetails");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SalaryCalculationId).IsRequired();
            entity.Property(e => e.SalaryHeadId).IsRequired();
            entity.Property(e => e.HeadName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.HeadCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.HeadType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CalculationMethod).IsRequired().HasMaxLength(30);
            entity.Property(e => e.BaseAmount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.AppliedPercentage).HasColumnType("decimal(6,4)");
            entity.Property(e => e.ComputedAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.SalaryCalculation)
                .WithMany(c => c.Details)
                .HasForeignKey(e => e.SalaryCalculationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SalaryCalculationId);
            entity.HasIndex(e => e.SalaryHeadId);
        });
    }
}
