using BusinessObjects.Common;

namespace DTOs.UserDTOs.Request
{
    public class UpdateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public string PhoneNumbers { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
