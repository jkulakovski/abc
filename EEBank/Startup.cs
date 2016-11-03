using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EEBank.Startup))]
namespace EEBank
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
