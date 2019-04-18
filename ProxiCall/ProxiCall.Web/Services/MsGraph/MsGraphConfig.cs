namespace ProxiCall.Web.Services.MsGraph
{
    public class MsGraphConfig
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }

        public string AuthorityFormat { get; set; }
        public string Scope { get; set; }

        public string QueryCalendarView { get; set; }
        public string QueryEvents { get; set; }
    }
}
