namespace HRM.Core.DTOs.LeaveApplication;

public class LeaveApplicationFilterDto
{
    public int? EmployeeId { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Year { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
