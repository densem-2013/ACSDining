using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ACSDining.Web.Startup))]
namespace ACSDining.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
