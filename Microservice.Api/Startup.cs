using FluentValidation.AspNetCore;
using MediatR;
using Microservice.Api.Configuration;
using Microservice.Api.Filters;
using Microservice.Db.Configuration;
using Microservice.HangfireBackgroundJobServer.Configuration;
using Microservice.Logic.BackgroundProcessing;
using Microservice.Logic.Configuration;
using Microservice.Logic.Orders.Validators;
using Microservice.RabbitMessageBroker.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microservice.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCorsRules()
                .AddControllers()
                .AddNewtonsoftJson()
                ;

            services
                .AddMvc(options => { options.Filters.Add<ValidationFilter>(); })
                .AddFluentValidation(config => config.RegisterValidatorsFromAssemblyContaining<CreateOrderValidator>())
                ;

            services
                .AddDatabase(_configuration.GetConnectionString("Database"))
                .AddLogic(_configuration)
                .AddMediatR(typeof(LogicServiceCollectionExtensions).Assembly)
                .AddRabbitMqMessageBroker(_configuration.GetSection("MessageBrokerSettings"))
                .AddHangfireBackgroundJobServer(_configuration.GetSection("BackgroundJobServerSettings"))
                .AddMessageBrokerCustomSubscriptions()
                .AddMessageBrokerCustomPublishers()
                .AddBackgroundProcessing(_configuration)
                ;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app
                .UseBackgroundJobServerDashboard()
                .UseHttpsRedirection()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                })
                ;
        }
    }
}
