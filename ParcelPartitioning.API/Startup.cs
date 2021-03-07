using FluentNHibernate.Cfg;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NHibernate;
using ParcelPartitioning.Data.Maps;
using System.Linq;

namespace ParcelPartitioning.API
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
            services.AddControllers();
            services.AddSwaggerGen();

            services.AddSingleton<ISessionFactory>(factory =>
            {
                return Fluently.Configure()
                .Database(
                    () =>
                    {
                        return FluentNHibernate.Cfg.Db.PostgreSQLConfiguration.Standard
                        .ShowSql()
                        .ConnectionString(Configuration.GetConnectionString("DefaultConfiguration"));
                    }
                )
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ParcelMap>())
                .BuildSessionFactory();
            });

            services.AddSingleton<ISession>(factory => factory.GetServices<ISessionFactory>().First().OpenSession());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
