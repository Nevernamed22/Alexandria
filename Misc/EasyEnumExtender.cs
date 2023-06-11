using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Alexandria.Misc
{
    public static class EasyEnumExtender
    {
        /// <summary>
        /// In each class with the [EnumExtension(enumType)] attribute sets each public and static field to a new value of the given enum, named after the field.
        /// </summary>
        /// <param name="guid">The guid of your mod.</param>
        /// <param name="asmbl">The assembly, the classes of which will be affected. If null, defaults to the assembly that called this method.</param>
        public static void ExtendEnumsInAssembly(string guid, Assembly asmbl = null)
        {
            asmbl ??= Assembly.GetCallingAssembly();
            if(asmbl != null)
            {
                foreach (var type in asmbl.GetTypes())
                {
                    var custom = type.GetCustomAttributes(false);
                    if (custom != null)
                    {
                        var extension = custom.OfType<EnumExtensionAttribute>().FirstOrDefault();
                        if (extension != null && extension.type != null && extension.type.IsEnum)
                        {
                            foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                            {
                                f.SetValue(null, ETGModCompatibility.ExtendEnum(guid, f.Name, extension.type));
                            }
                        }
                    }
                }
            }
        }
    }

    public class EnumExtensionAttribute : Attribute
    {
        public EnumExtensionAttribute(Type extensiontype)
        {
            type = extensiontype;
        }

        public Type type;
    }
}
