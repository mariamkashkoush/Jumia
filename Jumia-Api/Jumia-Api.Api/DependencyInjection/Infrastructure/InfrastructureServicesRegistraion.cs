﻿using Jumia_Api.Application.Interfaces;

using Jumia_Api.Application.Services;
using Jumia_Api.Domain.Models;

using Jumia_Api.Domain.Interfaces.Repositories;

using Jumia_Api.Infrastructure.Presistence.Context;
using Jumia_Api.Infrastructure.Presistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client;
using Jumia_Api.Infrastructure.External_Services;
using OllamaSharp;

namespace Jumia_Api.Api.DependencyInjection.Infrastructure
{
    public static class InfrastructureServicesRegistraion
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<JumiaDbContext>(op => op.UseSqlServer(configuration.GetConnectionString("JumiaContextConnection")));
            services.AddSignalR();
            services.AddScoped<IChatRepository, ChatRepository>();
            //services.AddScoped<IChatService, ChatService>();


            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<JumiaDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IProductAiService, ProductAiService > ();
            services.AddSingleton(new QdrantClient(host: "localhost", port: 6334, https: false));
            services.AddScoped<IConfirmationEmailService,ConfirmationEmailService>();
            services.AddSingleton(new OllamaApiClient("http://localhost:11434"));
            services.AddScoped<IEmailService, SendGridEmailService>();
            services.AddScoped<ICampaignEmailService, CampaignEmailService>(); 

            // Register the Background Worker
            services.AddHostedService<CampaignEmailWorker>();


            return services;

        }
    }
}
