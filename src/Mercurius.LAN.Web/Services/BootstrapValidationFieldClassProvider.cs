using Microsoft.AspNetCore.Components.Forms;

namespace Mercurius.LAN.Web.Services
{
    public class BootstrapValidationFieldClassProvider: FieldCssClassProvider
    {
        public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
        {
            bool isValid = editContext.IsValid(fieldIdentifier);
            bool isModified = editContext.IsModified(fieldIdentifier);

            if(!editContext.IsModified())
                return string.Empty;
            return $"{(isModified ? "modified " : "")}{(isValid ? "is-valid" : "is-invalid")}";
        }
    }
}
