namespace HRM.Core.DTOs.EmployeeSeparation;

public class SeparationFilterDto
{
    public string? SeparationType { get; set; }
    public string? Status { get; set; }
    public int? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
