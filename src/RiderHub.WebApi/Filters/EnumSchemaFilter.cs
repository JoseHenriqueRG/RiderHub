using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace RiderHub.WebApi.Filters
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                var enumValues = Enum.GetValues(context.Type).Cast<Enum>();
                schema.Enum.Clear();

                foreach (var value in enumValues)
                {
                    var display = value.GetType()
                                       .GetMember(value.ToString())
                                       .First()
                                       .GetCustomAttribute<DisplayAttribute>()?
                                       .Name ?? value.ToString();

                    schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(display));
                }
            }
        }
    }
}
