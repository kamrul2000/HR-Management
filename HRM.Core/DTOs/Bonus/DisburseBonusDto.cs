using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Bonus;

public class DisburseBonusDto
{
    /// <summary>
    /// Required when IsDisbursedWithSalary == true.
    /// Links the bonus to an existing salary calculation record.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? SalaryCalculationId { get; set; }
}
