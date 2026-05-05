namespace HRM.Core.DTOs.LeaveAllotment;

public class BulkCreateResultDto
{
    public int SuccessCount { get; set; }
    public int SkippedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> SkippedReasons { get; set; } = new();
    public List<string> FailedReasons { get; set; } = new();
}
