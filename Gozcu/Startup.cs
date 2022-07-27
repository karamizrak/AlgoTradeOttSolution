using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Gozcu.Startup))]
namespace Gozcu
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
