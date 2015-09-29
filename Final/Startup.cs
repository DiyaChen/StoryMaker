using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(My_lab7.Startup))]
namespace My_lab7
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
