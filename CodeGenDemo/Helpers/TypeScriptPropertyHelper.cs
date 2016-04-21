using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGenDemo
{ 
    public static class TypeScriptPropertyHelper
    {
        public static string Parse(EntityProperty property)
        {
            string line = string.Empty;

            if (property.Type.ToLower().Contains("int") || property.Type.ToLower().Contains("double"))
            {
                property.Type = "number";
                property.DefaultValue = "0";
            }

            if (property.Type.ToLower().Contains("date"))
            {
                property.Type = "Date";
                property.DefaultValue = "null";
            }

            if (property.Type.ToLower().Contains("string"))
            {
                property.Type = "string";
                property.DefaultValue = "null";
            }

            if (property.Type.ToLower().Contains("boolean"))
            {
                property.Type = "boolean";
                property.DefaultValue = "false";
            }

            if (property.IsArray)
            {
                property.Type = string.Format("Array<{0}>", property.Type);
                property.DefaultValue = "[]";
            }

            if (string.IsNullOrEmpty(property.DefaultValue))
            {
                property.DefaultValue = "null";
            }
            
            line = string.Format("public {0}: {1} = {2};{3}", property.Name, property.Type, property.DefaultValue, Environment.NewLine);

            return line;
        }
    }
}
