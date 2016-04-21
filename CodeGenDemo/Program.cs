using System;
using Microsoft.Extensions.Configuration;

namespace CodeGenDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            AppSettingsHelper.Configuration = builder.Build();

            var generator = new CodeGenerator();
            generator.Generate();

            Console.WriteLine("Done. Press key to continue...");
            Console.ReadKey();
        }
    }
}
