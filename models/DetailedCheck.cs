using System;

namespace QualityAppWPF.Models
{
    public class DetailedCheck
    {
        public int Id { get; set; }
        public int QualityCheckId { get; set; }
        public int ParameterId { get; set; }
        public decimal Value { get; set; }
        public bool IsPassed { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Навигационные свойства
        public string ParameterName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
    }
}