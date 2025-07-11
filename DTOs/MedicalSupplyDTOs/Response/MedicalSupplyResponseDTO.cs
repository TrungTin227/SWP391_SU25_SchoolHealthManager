﻿namespace DTOs.MedicalSupplyDTOs.Response
{
    public class MedicalSupplyResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "";
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public bool IsActive { get; set; }
        public bool IsLowStock => CurrentStock <= MinimumStock && MinimumStock > 0;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = "";
        public string UpdatedBy { get; set; } = "";
    }
}