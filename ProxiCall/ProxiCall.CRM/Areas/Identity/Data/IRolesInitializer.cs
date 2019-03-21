using System.Threading.Tasks;

namespace Proxicall.CRM.Areas.Identity.Data
{
    public interface IRolesInitializer
    {
        void Initialize();
        Task InitializeAsync();
    }
}
