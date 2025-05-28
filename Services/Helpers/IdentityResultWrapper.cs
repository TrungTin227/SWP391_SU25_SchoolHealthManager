using Microsoft.AspNetCore.Identity;

namespace Services.Helpers
{
    public class IdentityResultWrapper
    {
        public bool Succeeded { get; }
        public string ErrorMessage { get; }

        public IdentityResultWrapper(IdentityResult res)
        {
            Succeeded = res.Succeeded;
            ErrorMessage = res.Succeeded
                ? null
                : string.Join(", ", res.Errors.Select(e => e.Description));
        }
    }
}
