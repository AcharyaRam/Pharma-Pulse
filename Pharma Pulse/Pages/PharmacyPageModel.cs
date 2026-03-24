using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pharma_Pulse.Pages
{
    public class PharmacyPageModel : PageModel
    {
        protected int CurrentPharmacyId
        {
            get
            {
                var id = HttpContext.Session.GetInt32("PharmacyId");

                if (id == null)
                {
                    Response.Redirect("/Login");
                    return 0;
                }

                return id.Value;
            }
        }
    }
}