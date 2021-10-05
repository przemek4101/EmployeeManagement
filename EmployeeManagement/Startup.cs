using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement
{

    
    public class Startup
    {
        private IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

       

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 3;


            }).AddEntityFrameworkStores<AppDbContext>();

            services.AddMvc(options => {
                options.EnableEndpointRouting = false;
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                }).AddXmlSerializerFormatters();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "614120777865-j5d89gm71g62d5niaju6evckmreqb4pi.apps.googleusercontent.com";
                    options.ClientSecret = "Okil1WfN4Qh4ArjFhB5xjwyG";
                })
                .AddFacebook(options =>
                {
                    options.ClientId = "1026938824787842";
                    options.ClientSecret = "4ef53d40eb5b4ef4d87a7803cae8d7f6";
                });

           

            //Zmiana sciezki metody i widoku AccessDenied na /Administration/AccessDenied
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            }
            );

            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy",
                    policy => policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") &&
                   context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                   context.User.IsInRole("Super Admin")

                    ));

                options.AddPolicy("AdminRolePolicy",
                    policy => policy.RequireRole("Admin","true"));

                options.AddPolicy("EditRolePolicy",
                    policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

                //Ustawiam false jezeli nie chce, zeby pozostoa³e procedury obs³ugi by³y wywo³ywane po zwróceniu niepowodzenia. Domyœlnie true.
                //options.InvokeHandlersAfterFailure = false;
                
                
            });
            
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            app.UseStaticFiles();
            
            app.UseRouting();
            app.UseAuthentication();//Wa¿ne zeby by³o przed UseMvc
            app.UseMvc(routes =>
            {
                //domyslna strona
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });



            //app.UseMvcWithDefaultRoute();
           

            app.UseEndpoints(endpoints =>
            {
            });
        }

        
    }
}
