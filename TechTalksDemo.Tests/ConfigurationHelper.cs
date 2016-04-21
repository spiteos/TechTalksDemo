using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalksDemo.Helpers;
using Microsoft.Extensions.Configuration;

namespace TechTalksDemo.Tests
{
    public static class ConfigurationHelper
    {
        public static void Ensure()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            
            AppSettingsHelper.Configuration = builder.Build();
        }
    }
}
