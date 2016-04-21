using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechTalksDemo.Models;

namespace CodeGenDemo
{
    public class CodeGenerator
    {
        private List<MyEntity> entities { get; set; }
        private string entitiesFromConfiguration = AppSettingsHelper.Configuration["CodeGenerator:Entities"];
        private string typescriptPlaceholder = AppSettingsHelper.Configuration["CodeGenerator:TypescriptPlaceholder"];
        private bool overwriteExistingFile = AppSettingsHelper.Configuration["CodeGenerator:OverwriteExistingFiles"].Equals("true", StringComparison.OrdinalIgnoreCase);
        private bool outputToConsole = AppSettingsHelper.Configuration["CodeGenerator:OutputToConsole"].Equals("true", StringComparison.OrdinalIgnoreCase);

        public void Generate()
        {
            this.GetEntities();
            this.GenerateFilesForEntities("Models");
            this.GenerateFilesForEntities("ModelMappings");

            this.GenerateFilesForEntities("Repositories");
            this.GenerateFilesForEntities("Services");

            this.GenerateFilesForEntities("ApiControllers");
            //this.GenerateFilesForEntities("PublicControllers");

            this.GenerateFilesForEntities("TestServices");
            this.GenerateFilesForEntities("TestIntegrations");
            //this.GenerateFilesForEntities("TestPublicIntegrations");

            //this.GetEntityProperties();
            //this.GenerateFilesForEntities("TypeScriptModels", true);
        }

        private void GetEntities()
        {
            this.entities = new List<MyEntity>();
            var entityNames = entitiesFromConfiguration.Split(',').ToList();
            foreach(var entityName in entityNames)
            {
                var entity = new MyEntity()
                {
                    Name = entityName,
                    ExistsInContext = this.CheckExistsInContext(entityName),
                    Properties = new List<EntityProperty>()
                };

                this.entities.Add(entity);
            }
        }

        private bool CheckExistsInContext(string entityName)
        {
            var exists = false;
#if DNX451
            var sets = typeof(ApplicationDbContext).GetProperties();

            foreach (var set in sets)
            {
                var genericType = set.PropertyType.GenericTypeArguments.FirstOrDefault();
                if (genericType != null && 
                    !string.IsNullOrEmpty(genericType.Name) && 
                    genericType.Name.Equals(entityName)
                   )
                {
                    exists = true;
                }
            }

            Console.WriteLine("CheckExistsInContext for '{0}': {1} in context.", entityName, exists ? "Exists" : "Doesn't exist");
            return exists;
#endif
            return true;
        }


        private void GenerateFilesForEntities(string configSection, bool isTypeScriptModel = false)
        {
            var modelConfiguration = new CodeGeneratorConfiguration(configSection);

            Directory.CreateDirectory(modelConfiguration.Folder);

            foreach (var entity in this.entities)
            {
                string text = File.ReadAllText(modelConfiguration.TemplatePath);
                text = text.Replace(modelConfiguration.Placeholder, entity.Name);

                //overwrite file decision
                if (overwriteExistingFile || !File.Exists(string.Format(modelConfiguration.FilePathTemplate, modelConfiguration.Folder, entity.Name)))
                {
                    // we can do in memory compiling... but don't want to:)
                    //var tree = SyntaxFactory.ParseSyntaxTree(text);

                    string entityName = entity.Name;

                    if (isTypeScriptModel)
                    {
                        text = this.AddTypeScriptProperties(text, entity);
                        entityName = entityName.ToLower();
                    }

                    string outputFilePath = string.Format(modelConfiguration.FilePathTemplate, modelConfiguration.Folder, entityName);
                    if (modelConfiguration.SkipIfModelExists)
                    {
                        if (!entity.ExistsInContext)
                        {
                            if (this.outputToConsole)
                            {
                                Console.WriteLine(text);
                            }
                            else
                            {
                                Console.WriteLine("Generating '{0}' in '{1}' - doesn't exist in context.", outputFilePath, configSection);
                                File.WriteAllText(outputFilePath, text);
                            }                            
                        }
                    }
                    else
                    {
                        if (this.outputToConsole)
                        {
                            Console.WriteLine(text);
                        }
                        else
                        {
                            Console.WriteLine("Generating '{0}' in '{1}'.", outputFilePath, configSection);
                            File.WriteAllText(outputFilePath, text);
                        }
                    }
                }
            }
        }

        private void GetEntityProperties()
        {
#if DNX451
            foreach (var entity in this.entities)
            {
                Type entityType = Type.GetType(string.Format("TechTalksDemo.Models.{0},TechTalksDemo", entity.Name));

                var properties = entityType.GetProperties();

                foreach (var property in properties)
                {
                    var entityProperty = new EntityProperty()
                    {
                        Name = property.Name,
                        Type = property.PropertyType.Name,
                        IsNullable = false,
                        IsArray = false
                    };

                    if (entityProperty.Type.Contains("`1"))
                    {
                        if (entityProperty.Type.Contains("Nullable"))
                        {
                            entityProperty.IsNullable = true;
                            entityProperty.DefaultValue = "null";
                        }

                        if (entityProperty.Type.Contains("List") ||
                            entityProperty.Type.Contains("HashSet"))
                        {
                            entityProperty.IsArray = true;
                            entityProperty.DefaultValue = "[]";
                        }

                        var genericType = property.PropertyType.GenericTypeArguments.FirstOrDefault();
                        if (genericType != null && !string.IsNullOrEmpty(genericType.Name))
                        {
                            entityProperty.Type = genericType.Name;
                        }
                    }

                    entity.Properties.Add(entityProperty);
                }
            }
#endif
        }

        private string AddTypeScriptProperties(string text, MyEntity entity)
        {
            string allProperties = string.Empty;

            foreach(var property in entity.Properties)
            {
                allProperties = allProperties + TypeScriptPropertyHelper.Parse(property);
            }

            text = text.Replace(this.typescriptPlaceholder, allProperties);
            return text;
        }
    }

    public class CodeGeneratorConfiguration
    {
        public CodeGeneratorConfiguration(string configurationSection)
        {
            this.SkipIfModelExists = AppSettingsHelper.Configuration[string.Format("CodeGenerator:{0}:SkipIfModelExists", configurationSection)]
                .Equals("false", StringComparison.OrdinalIgnoreCase) ? false : true;

            this.Folder = AppSettingsHelper.Configuration[string.Format("CodeGenerator:{0}:Folder", configurationSection)];
            this.TemplatePath = AppSettingsHelper.Configuration[string.Format("CodeGenerator:{0}:TemplatePath", configurationSection)];
            this.Placeholder = AppSettingsHelper.Configuration[string.Format("CodeGenerator:{0}:Placeholder", configurationSection)];
            this.FilePathTemplate = AppSettingsHelper.Configuration[string.Format("CodeGenerator:{0}:FilePathTemplate", configurationSection)];
        }

        public bool SkipIfModelExists { get; set; }
        public string Folder { get; set; }
        public string TemplatePath { get; set; }
        public string Placeholder { get; set; }
        public string FilePathTemplate { get; set; }
    }

    public class EntityProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public bool IsNullable { get; set; }
        public bool IsArray { get; set; }
    }

    public class MyEntity
    {
        public string Name { get; set; }
        public bool ExistsInContext { get; set; }
        public List<EntityProperty> Properties { get; set; }
    }
}
