using System;

namespace QualityAppWPF.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public int? ManufacturingId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Новые поля для БЖУ
        public double? Protein { get; set; } // Белки (г на 100г)
        public double? Fat { get; set; }     // Жиры (г на 100г)
        public double? Carbs { get; set; }   // Углеводы (г на 100г)
        public double? Calories { get; set; } // Калории (ккал на 100г)
        public string NutritionInfo
        {
            get
            {
                if (Protein.HasValue || Fat.HasValue || Carbs.HasValue)
                {
                    string result = "";
                    if (Protein.HasValue) result += $"Б: {Protein:F1}г ";
                    if (Fat.HasValue) result += $"Ж: {Fat:F1}г ";
                    if (Carbs.HasValue) result += $"У: {Carbs:F1}г";
                    return result.Trim();
                }
                return "Не указано";
            }
        }
        public string CaloriesInfo => Calories.HasValue ? $"{Calories:F0} ккал" : "—";
    }
}