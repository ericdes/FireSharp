using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FireSharp
{
    internal enum PathedObjectType
    {
        Root,
        Property,
        DictionaryItem,
        ListItem,
    }

    internal class PathedObjectInfo
    {
        /// <summary>
        ///  Path to the object
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Instance name of the object
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Our in-house differentiation of the instance's type
        /// </summary>
        public PathedObjectType Type { get; private set; }

        /// <param name="path">JSON path of the target object</param>
        /// <param name="name">Related property name</param>
        /// <param name="parentObjectType">Related property type</param>
        public PathedObjectInfo(string path, string name, Type parentObjectType, out PropertyInfo propertyInfo)
        {
            this.Path = path;
            this.Name = name;
            propertyInfo = null;
            if (typeof(IDictionary).IsAssignableFrom(parentObjectType))
            {
                this.Type = PathedObjectType.DictionaryItem;
            }
            else if (typeof(IList).IsAssignableFrom(parentObjectType))
            {
                this.Type = PathedObjectType.ListItem;
            }
            else if (string.IsNullOrEmpty(name))
            {
                // Target object is root
                this.Type = PathedObjectType.Root;
            }
            else
            {
                propertyInfo = parentObjectType.GetProperty(name);
                if (propertyInfo == null)
                {
                    throw new InvalidOperationException(string.Format("Property '{0}' in {1} not found.", name, parentObjectType));
                }
                this.Type = PathedObjectType.Property;
            }
        }
    }



}
