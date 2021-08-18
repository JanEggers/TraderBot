using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TraderBot.Extensions;

namespace TraderBot
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) 
        {

            services.AddTraderBot(Secrets.ApiKey);

            services.AddMvc();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapSwagger();
            });

            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
