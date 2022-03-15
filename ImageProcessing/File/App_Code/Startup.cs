using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(File.Startup))]
namespace File
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
