using APIClient.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net;
using System.Net.Http;

namespace APIClient
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
            services.AddScoped<IAPIWhoSayNiClient, APIWhoSayNiClient>();

            services.AddHttpClient("APIWhoSayNi", client =>
            {
                client.BaseAddress = new Uri(Configuration["EndpointAPIWhoSayNi"]);
            })
                .AddPolicyHandler(GetRetryPolicy());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // It does not work, look README.md in repository
        //
        //private static IAsyncPolicy<HttpResponseMessage> GetUnauthorizedPolicy(IServiceCollection services) =>
        //    HttpPolicyExtensions
        //        .HandleTransientHttpError()
        //        .OrResult(msg => msg.StatusCode == HttpStatusCode.Unauthorized)
        //        .RetryAsync(1, async (response, retryCount, context) =>
        //        {
        //            var provider = services.BuildServiceProvider();
        //            var client = provider.GetRequiredService<IAPIWhoSayNiClient>();
        //            var token = await client.RefreshAuthenticationToken(new CancellationToken());
        //        });
    }
}