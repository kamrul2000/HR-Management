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
    public DbSet<BonusCalculation> BonusCalculations => Set<BonusCalculation>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<LoanRecommendation> LoanRecommendations => Set<LoanRecommendation>();
    public DbSet<LoanApproval> LoanApprovals => Set<LoanApproval>();
    public DbSet<EmployeeLoan> EmployeeLoans => Set<EmployeeLoan>();
    public DbSet<LoanInstallment> LoanInstallments => Set<LoanInstallment>();
    public DbSet<TaxSlabConfig> TaxSlabConfigs => Set<TaxSlabConfig>();
    public DbSet<TaxSlab> TaxSlabs => Set<TaxSlab>();
    public DbSet<TaxExclusion> TaxExclusions => Set<TaxExclusion>();
    public DbSet<PfRule> PfRules => Set<PfRule>();
    public DbSet<EmployeePfContribution> EmployeePfContributions => Set<EmployeePfContribution>();
    public DbSet<PfInterestRate> PfInterestRates => Set<PfInterestRate>();
    public DbSet<EmployeePfInterest> EmployeePfInterests => Set<EmployeePfInterest>();
    public DbSet<GratuityRule> GratuityRules => Set<GratuityRule>();
    public DbSet<GratuityCalculation> GratuityCalculations => Set<GratuityCalculation>();
    public DbSet<SeparationReason> SeparationReasons => Set<SeparationReason>();
    public DbSet<EmployeeSeparation> EmployeeSeparations => Set<EmployeeSeparation>();

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

        modelBuilder.Entity<BonusCalculation>(entity =>
        {
            entity.ToTable("BonusCalculations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.BonusType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BonusTitle).IsRequired().HasMaxLength(150);
            entity.Property(e => e.CalculationBasis).IsRequired().HasMaxLength(30);
            entity.Property(e => e.BasisPercentage).HasColumnType("decimal(6,4)");
            entity.Property(e => e.BasicSalarySnapshot).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.GrossSalarySnapshot).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.ComputedAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.FinalAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.DisbursementMonth).IsRequired();
            entity.Property(e => e.DisbursementYear).IsRequired();
            entity.Property(e => e.IsDisbursedWithSalary).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ApprovalRemarks).HasMaxLength(500);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SalaryCalculation)
                .WithMany()
                .HasForeignKey(e => e.SalaryCalculationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.DisbursementYear, e.DisbursementMonth });
        });

        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.ToTable("LoanApplications");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ApplicationNo).IsRequired().HasMaxLength(30);
            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.LoanType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RequestedAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.RequestedTenureMonths).IsRequired();
            entity.Property(e => e.Purpose).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.AttachmentPath).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
            entity.Property(e => e.RecommendationRemarks).HasMaxLength(500);
            entity.Property(e => e.RejectionRemarks).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.ApplicationNo, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<LoanRecommendation>(entity =>
        {
            entity.ToTable("LoanRecommendations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.LoanApplicationId).IsRequired();
            entity.Property(e => e.RecommendedById).IsRequired();
            entity.Property(e => e.Decision).IsRequired().HasMaxLength(20);
            entity.Property(e => e.RecommendedAmount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.Remarks).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.LoanApplication)
                .WithMany()
                .HasForeignKey(e => e.LoanApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.LoanApplicationId);
            entity.HasIndex(e => new { e.LoanApplicationId, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<LoanApproval>(entity =>
        {
            entity.ToTable("LoanApprovals");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.LoanApplicationId).IsRequired();
            entity.Property(e => e.ApprovedById).IsRequired();
            entity.Property(e => e.Decision).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ApprovedAmount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.MonthlyInstallment).HasColumnType("decimal(12,2)");
            entity.Property(e => e.InterestRate).HasColumnType("decimal(6,4)");
            entity.Property(e => e.InterestType).HasMaxLength(20);
            entity.Property(e => e.Remarks).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.LoanApplication)
                .WithMany()
                .HasForeignKey(e => e.LoanApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.LoanApplicationId);
            entity.HasIndex(e => new { e.LoanApplicationId, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<EmployeeLoan>(entity =>
        {
            entity.ToTable("EmployeeLoans");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.LoanApplicationId).IsRequired();
            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.LoanNo).IsRequired().HasMaxLength(30);
            entity.Property(e => e.PrincipalAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.InterestRate).HasColumnType("decimal(6,4)").IsRequired();
            entity.Property(e => e.InterestType).HasMaxLength(20);
            entity.Property(e => e.TenureMonths).IsRequired();
            entity.Property(e => e.MonthlyInstallment).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.TotalRepayable).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.DisbursementDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.FirstInstallmentMonth).IsRequired();
            entity.Property(e => e.FirstInstallmentYear).IsRequired();
            entity.Property(e => e.TotalPaid).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.OutstandingBalance).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.PaidInstallments).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.LoanApplication)
                .WithMany()
                .HasForeignKey(e => e.LoanApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.LoanApplicationId, e.SubscriptionId }).IsUnique();
            entity.HasIndex(e => new { e.LoanNo, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<LoanInstallment>(entity =>
        {
            entity.ToTable("LoanInstallments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeLoanId).IsRequired();
            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.InstallmentNo).IsRequired();
            entity.Property(e => e.DueMonth).IsRequired();
            entity.Property(e => e.DueYear).IsRequired();
            entity.Property(e => e.InstallmentAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.EmployeeLoan)
                .WithMany(l => l.Installments)
                .HasForeignKey(e => e.EmployeeLoanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SalaryCalculation)
                .WithMany()
                .HasForeignKey(e => e.SalaryCalculationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeLoanId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.EmployeeId, e.DueYear, e.DueMonth, e.Status });
        });

        modelBuilder.Entity<TaxSlabConfig>(entity =>
        {
            entity.ToTable("TaxSlabConfigs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FiscalYear).IsRequired().HasMaxLength(10);
            entity.Property(e => e.StartDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.EndDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.TaxFreeThreshold).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => new { e.FiscalYear, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<TaxSlab>(entity =>
        {
            entity.ToTable("TaxSlabs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TaxSlabConfigId).IsRequired();
            entity.Property(e => e.SlabOrder).IsRequired();
            entity.Property(e => e.MinAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.MaxAmount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.TaxRate).HasColumnType("decimal(6,4)").IsRequired();
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.TaxSlabConfig)
                .WithMany(c => c.Slabs)
                .HasForeignKey(e => e.TaxSlabConfigId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TaxSlabConfigId);
        });

        modelBuilder.Entity<TaxExclusion>(entity =>
        {
            entity.ToTable("TaxExclusions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ExclusionType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PartialExclusionAmount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.EffectiveFrom).HasColumnType("date").IsRequired();
            entity.Property(e => e.EffectiveTo).HasColumnType("date");
            entity.Property(e => e.CertificateNo).HasMaxLength(100);
            entity.Property(e => e.AttachmentPath).HasMaxLength(500);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => new { e.EmployeeId, e.IsActive, e.EffectiveFrom });
        });

        modelBuilder.Entity<PfRule>(entity =>
        {
            entity.ToTable("PfRules");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RuleName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EmployeeContributionRate).HasColumnType("decimal(6,4)").IsRequired();
            entity.Property(e => e.EmployerContributionRate).HasColumnType("decimal(6,4)").IsRequired();
            entity.Property(e => e.PfBase).IsRequired().HasMaxLength(30);
            entity.Property(e => e.MinEligibleSalary).HasColumnType("decimal(12,2)");
            entity.Property(e => e.MaxContributionAmount).HasColumnType("decimal(12,2)");
            entity.Property(e => e.EffectiveFrom).HasColumnType("date").IsRequired();
            entity.Property(e => e.EffectiveTo).HasColumnType("date");
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => new { e.RuleName, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<EmployeePfContribution>(entity =>
        {
            entity.ToTable("EmployeePfContributions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.PfRuleId).IsRequired();
            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.Month).IsRequired();
            entity.Property(e => e.PfBase).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.EmployeeContributionRate).HasColumnType("decimal(6,4)").IsRequired();
            entity.Property(e => e.EmployerContributionRate).HasColumnType("decimal(6,4)").IsRequired();
            entity.Property(e => e.EmployeeContribution).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.EmployerContribution).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.TotalContribution).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PfRule)
                .WithMany()
                .HasForeignKey(e => e.PfRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SalaryCalculation)
                .WithMany()
                .HasForeignKey(e => e.SalaryCalculationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.PfRuleId);
            entity.HasIndex(e => new { e.EmployeeId, e.Year, e.Month, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<PfInterestRate>(entity =>
        {
            entity.ToTable("PfInterestRates");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FiscalYear).IsRequired().HasMaxLength(10);
            entity.Property(e => e.InterestRate).HasColumnType("decimal(6,4)").IsRequired();
            entity.Property(e => e.EffectiveFrom).HasColumnType("date").IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => new { e.FiscalYear, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<EmployeePfInterest>(entity =>
        {
            entity.ToTable("EmployeePfInterests");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.PfInterestRateId).IsRequired();
            entity.Property(e => e.FiscalYear).IsRequired().HasMaxLength(10);
            entity.Property(e => e.OpeningBalance).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.TotalContributionsForYear).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.AverageBalance).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.InterestRate).HasColumnType("decimal(6,4)").IsRequired();
            entity.Property(e => e.InterestAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.ClosingBalance).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PfInterestRate)
                .WithMany()
                .HasForeignKey(e => e.PfInterestRateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.PfInterestRateId);
            entity.HasIndex(e => new { e.EmployeeId, e.FiscalYear, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<GratuityRule>(entity =>
        {
            entity.ToTable("GratuityRules");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RuleName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MinServiceYears).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(e => e.CalculationBasis).IsRequired().HasMaxLength(30);
            entity.Property(e => e.RatePerYear).HasColumnType("decimal(6,2)").IsRequired();
            entity.Property(e => e.MaxGratuityAmount).HasColumnType("decimal(14,2)");
            entity.Property(e => e.MaxServiceYearsCapped);
            entity.Property(e => e.ProRataEnabled).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.EffectiveFrom).HasColumnType("date").IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => new { e.RuleName, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<GratuityCalculation>(entity =>
        {
            entity.ToTable("GratuityCalculations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.GratuityRuleId).IsRequired();
            entity.Property(e => e.SeparationDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.JoiningDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.TotalServiceDays).IsRequired();
            entity.Property(e => e.TotalServiceYears).HasColumnType("decimal(8,4)").IsRequired();
            entity.Property(e => e.EligibleYears).HasColumnType("decimal(8,4)").IsRequired();
            entity.Property(e => e.CalculationBasis).IsRequired().HasMaxLength(30);
            entity.Property(e => e.MonthlySalaryUsed).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.DailySalary).HasColumnType("decimal(12,4)").IsRequired();
            entity.Property(e => e.RatePerYear).HasColumnType("decimal(6,2)").IsRequired();
            entity.Property(e => e.GratuityBeforeCap).HasColumnType("decimal(14,2)").IsRequired();
            entity.Property(e => e.GratuityAmount).HasColumnType("decimal(14,2)").IsRequired();
            entity.Property(e => e.IsCapApplied).IsRequired();
            entity.Property(e => e.IsEligible).IsRequired();
            entity.Property(e => e.IneligibilityReason).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.GratuityRule)
                .WithMany()
                .HasForeignKey(e => e.GratuityRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.GratuityRuleId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<SeparationReason>(entity =>
        {
            entity.ToTable("SeparationReasons");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ReasonName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Category).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => new { e.ReasonName, e.SubscriptionId }).IsUnique();
        });

        modelBuilder.Entity<EmployeeSeparation>(entity =>
        {
            entity.ToTable("EmployeeSeparations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EmployeeId).IsRequired();
            entity.Property(e => e.SeparationReasonId).IsRequired();
            entity.Property(e => e.SeparationType).IsRequired().HasMaxLength(30);
            entity.Property(e => e.ApplicationDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.LastWorkingDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.NoticePeriodDays).IsRequired();
            entity.Property(e => e.ActualNoticeDays).IsRequired();
            entity.Property(e => e.NoticePeriodShortfall).IsRequired();
            entity.Property(e => e.NoticePeriodBuyout).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.GratuityAmount).HasColumnType("decimal(14,2)").IsRequired();
            entity.Property(e => e.OtherSettlementAmount).HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(e => e.TotalSettlementAmount).HasColumnType("decimal(14,2)").IsRequired();
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.AttachmentPath).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ApprovalRemarks).HasMaxLength(500);
            entity.Property(e => e.SubscriptionId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SeparationReason)
                .WithMany()
                .HasForeignKey(e => e.SeparationReasonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SubscriptionId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastWorkingDate);
        });
    }
}
