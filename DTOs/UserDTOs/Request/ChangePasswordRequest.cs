using System.ComponentModel.DataAnnotations;

namespace DTOs.UserDTOs.Request
{
    public class ChangePasswordRequest
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter your password")]
        public string OldPassword { get; set; } = "";
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter your new password")]
        [Compare("ConfirmPassword")]
        public string NewPassword { get; set; } = "";
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter your new password")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = "";
    }
}
