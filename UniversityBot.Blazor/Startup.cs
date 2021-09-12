using System;
using System.Text;
using System.Threading.Tasks;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using UniversityBot.Blazor.Data;
using UniversityBot.EF;
using UniversityBot.EF.Extension;
using UniversityBot.ServiceProvider.Configuration;
using UniversityBot.Sqlite;

namespace UniversityBot.Blazor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration; 
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            
            services
                .AddAntDesign()
                .AddSingleton<ObjectPool<StringBuilder>>(_ => new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy()));
            
            var dbFilePath = Configuration["DbFile"];
            var connectionString = SqliteContextFactory.BuildConnectionString(dbFilePath);
            var sqlDbFactory = new SqliteContextFactory(connectionString, true);
            using var db = sqlDbFactory.Create(optionConfigurator: null);
            services.ConfigureUniversityBotCore(db, sqlDbFactory, false);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider sp)
        {
            new MapperRegister().Register();

            using (var ctx = sp.GetRequiredService<AppDbContext>())
            {
                ctx.Database.EnsureCreated();
            }
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            if (HybridSupport.IsElectronActive)
            {
                Task.Run(async () =>
                {
                    await Electron.WindowManager.CreateBrowserViewAsync();
                    await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
                    {
                        MinWidth = 700,
                        MinHeight = 500,
                        Center = true
                    });
                });   
            }
        }
    }
}