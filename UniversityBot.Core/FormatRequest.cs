using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Blowin.Optional;

namespace UniversityBot.Core
{
    public class FormatMetadata : Attribute
    {
        public const string DefaultDateTimeFormat = "dd-MM-yyyy HH:mm";
        
        public string UserFriendlyName { get; }
        public string DefaultFormat { get; }

        public FormatMetadata(string userFriendlyName, string defaultFormat = null)
        {
            UserFriendlyName = userFriendlyName;
            DefaultFormat = defaultFormat;
        }
    }
    
    public sealed class FormatRequest
    {
        public static readonly IReadOnlyList<MetadataDescription> Metadata = typeof(FormatRequest).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Select(e => (Property: e, Attribute: e.GetCustomAttribute<FormatMetadata>()))
            .Where(e => e.Attribute != null)
            .Select(e => new MetadataDescription(e.Attribute.UserFriendlyName, e.Property.Name, e.Attribute.DefaultFormat.AsOptional()))
            .ToArray();

        [FormatMetadata("Имя пользователя")]
        public string UserName { get; init; }
            
        [FormatMetadata("Текущее время", FormatMetadata.DefaultDateTimeFormat)]
        public DateTime NowTime { get; init; }
        
        [FormatMetadata("Текущее время в Utc", FormatMetadata.DefaultDateTimeFormat)]
        public DateTime UtcTime => DateTime.UtcNow;

        public sealed class MetadataDescription
        {
            public string UserFriendlyName { get; }
            public string Property { get; }
            public Optional<string> DefaultFormat { get; }

            public MetadataDescription(string userFriendlyName, string property, Optional<string> defaultFormat)
            {
                UserFriendlyName = userFriendlyName;
                Property = property;
                DefaultFormat = defaultFormat;
            }

            public StringBuilder Append(StringBuilder builder)
            {
                builder.Append('{').Append(Property);
                if (DefaultFormat.IsSome)
                    builder.Append(':').Append(DefaultFormat.Value);
                
                return builder.Append('}');
            }
        }
    }
}