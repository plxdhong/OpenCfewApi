using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CFEW.Shared;
using Microsoft.Extensions.Logging;
using CFEW.Server.Models;
using Microsoft.Extensions.Options;
using CFEW.Server.Services;
using IdentityModel.AspNetCore.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdentityModel;

namespace CFEW.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            

            services.AddLogging();

            services.Configure<XrayUsersstoreDatabaseSettings>(
                Configuration.GetSection(nameof(XrayUsersstoreDatabaseSettings)));

            services.AddSingleton<IXrayUsersstoreDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<XrayUsersstoreDatabaseSettings>>().Value);

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddCors(option =>
            {
                option.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(Configuration["Origins"])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddControllers();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                // JWT tokens
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration["Authority"];
                    options.TokenValidationParameters.ValidateAudience = false;

                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };

                    // if token does not contain a dot, it is a reference token
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("CFEWApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "api_cfew");
                });
            });
            services.AddSingleton<XrayGrpcService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<XrayConfigService>();



            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else
            {   //全局异常处理
                _ = app.UseExceptionHandler(errorApp =>
                  {
                      errorApp.Run(async context =>
                      {
                          context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                          context.Response.ContentType = "application/json";
                          var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                          if (contextFeature != null)
                          {
                              logger.LogError($"Something went wrong: {contextFeature.Error}");
                              await context.Response.WriteAsync(new ErrorDetails()
                              {
                                  StatusCode = context.Response.StatusCode,
                                  Message = "Internal Server Error."
                              }.ToString());
                          }
                      });
                  });
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization("CFEWApiScope");
            });
        }
    }
}
