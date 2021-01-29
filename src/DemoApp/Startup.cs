using GraphqlToTsql;
using GraphqlToTsql.Database;
using GraphqlToTsql.Translator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DemoApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddControllers();

            services
                .AddScoped<IConnectionStringProvider, DemoConnectionStringProvider>()
                .AddScoped<IDataMutator, DataMutator>()
                .AddScoped<IDbAccess, DbAccess>()
                .AddScoped<IRunner, Runner>()
                .AddScoped<IListener, Listener>()
                .AddScoped<IParser, Parser>()
                .AddScoped<IQueryTreeBuilder, QueryTreeBuilder>()
                .AddScoped<ITsqlBuilder, TsqlBuilder>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
