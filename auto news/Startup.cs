using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.Dashboard;

[assembly: OwinStartup(typeof(auto_news.Startup))]

namespace auto_news
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            

            ConfigureAuth(app);


            //Hangefire configuration
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            var options = new DashboardOptions
            {
                AuthorizationFilters = new[]
                {
                    new AuthorizationFilter{ Users = "admin@gmail.com", Roles = "admin" }
                }
            };

            app.UseHangfireDashboard("/hangfire", options);

            app.UseHangfireServer();
            //End Hagefire configuration

        }
    }

}
