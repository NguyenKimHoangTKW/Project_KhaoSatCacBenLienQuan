using System.Web.Mvc;

namespace CTDT.Areas.CBVC
{
    public class CBVCAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "CBVC";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "CBVC_default",
                "CBVC/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}