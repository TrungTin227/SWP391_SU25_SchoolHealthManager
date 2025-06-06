﻿using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class Report : BaseEntity
    {
        [Key] public Guid Id { get; set; }
        public Guid? HealthEventId { get; set; }
        public HealthEvent? HealthEvent { get; set; }
        [MaxLength(200)] public string Title { get; set; }
        public ReportType ReportType { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string Data { get; set; }
    }
}
