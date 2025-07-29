using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class MedicalSupply : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Required, MaxLength(50)]
        public string Unit { get; set; } = ""; // e.g., "pcs", "box", "bottle"
        //public int CurrentStock { get; set; } 
        public int MinimumStock { get; set; }

        public bool IsActive { get; set; }

        // Navigation
        public virtual ICollection<MedicalSupplyLot> Lots { get; set; } = new List<MedicalSupplyLot>();
        [NotMapped]
        public int CurrentStock
        {
            get
            {
                return Lots.Where(lot => !lot.IsDeleted && lot.ExpirationDate > DateTime.UtcNow)
                           .Sum(lot => lot.Quantity);
            }
        }
        /// <summary>
        /// Kiểm tra xem vật tư có đang ở dưới mức tồn kho tối thiểu không.
        /// </summary>
        /// <returns>True nếu tồn kho thấp, ngược lại là False.</returns>
        public bool IsLowOnStock()
        {
            return CurrentStock < MinimumStock;
        }

        /// <summary>
        /// Tìm lô hàng phù hợp nhất để sử dụng (theo quy tắc FEFO - Hết hạn trước, dùng trước).
        /// </summary>
        /// <returns>Lô hàng có ngày hết hạn gần nhất hoặc null nếu hết hàng.</returns>
        public MedicalSupplyLot? FindBestLotToUse()
        {
            if (Lots == null || !Lots.Any()) return null;

            return Lots
                .Where(lot => lot.Quantity > 0 && lot.ExpirationDate > DateTime.UtcNow) // Chỉ tìm lô còn hàng và còn hạn
                .OrderBy(lot => lot.ExpirationDate) // Sắp xếp theo ngày hết hạn tăng dần
                .FirstOrDefault(); // Lấy lô có HSD gần nhất
        }

        /// <summary>
        /// Giảm số lượng từ một lô hàng cụ thể.
        /// </summary>
        /// <param name="lotId">ID của lô hàng cần sử dụng.</param>
        /// <param name="quantityToUse">Số lượng cần sử dụng.</param>
        /// <returns>True nếu thành công, False nếu lô không tồn tại hoặc không đủ hàng.</returns>
        public bool UseFromLot(Guid lotId, int quantityToUse)
        {
            var lot = Lots?.FirstOrDefault(l => l.Id == lotId);

            if (lot == null || lot.Quantity < quantityToUse)
            {
                return false; // Không tìm thấy lô hoặc không đủ số lượng
            }

            lot.Quantity -= quantityToUse;
            return true;
        }
    }
}
