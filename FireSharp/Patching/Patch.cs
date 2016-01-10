using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using FireSharp.Interfaces;
using FireSharp;

using Newtonsoft.Json;
using System.Reflection;
using System.Xml.Linq;
using System.Collections;
using FireSharp.Exceptions;

namespace FireSharp
{
    public partial class JsonPatchManager
    {
        /// <summary>
        /// JSON patch method. The object in paramater gets patched against the JSON patch.
        /// </summary>
        /// <typeparam name="T">Type of target object to patch</typeparam>
        /// <see cref="https://tools.ietf.org/html/rfc6902"/>
        public void Patch<T>(T target, JsonPatch jsonPatch)
        {
            var segments = jsonPatch.Path.Trim(new[] { '/' }).Split(new[] { '/' });

            InternalPatchRecursive(segments, jsonPatch, target, typeof(T));
        }

        private void InternalPatchRecursive(string[] pathSegments, JsonPatch patch, object value, Type valueType, int startingPathIndex = 0)
        {
            if (pathSegments.Length == 0) return;
            #region What is the current path?
            string currentPath = null;
            for (int i = 0; i <= startingPathIndex; i++)
            {
                currentPath = i == 0 ? pathSegments[0] : (currentPath + "/" + pathSegments[i]);
            }
#if DEBUG
            if (currentPath == "TELAERP_v3/Software/Developper/HomeAddress/AddressLines")
            {
            }
#endif
            #endregion
            // The property target to patch:
            PropertyInfo targetProperty;
            var target = new PathedObjectInfo(currentPath, pathSegments[startingPathIndex], valueType, out targetProperty);

            var endRecursion = startingPathIndex >= pathSegments.Length - 1;

            if (!endRecursion && target.Type == PathedObjectType.Property)
            {
                var targetValue = targetProperty.GetValue(value);
                if (targetValue == null)
                {
                    try
                    {
                        targetValue = Activator.CreateInstance(targetProperty.PropertyType);
                        targetProperty.SetValue(value, targetValue);
                    }
                    catch(Exception e)
                    {
                        // RFC6902: We don't have to succeed here
                        throw new InvalidOperationException(string.Format("Target location '{0}' does not exist.", target.Path), e); 
                    }
                }
                InternalPatchRecursive(pathSegments, patch, targetValue, targetProperty.PropertyType, startingPathIndex + 1); // Recurse
            }
            else if (endRecursion && target.Type == PathedObjectType.Root)
            {
                if (patch.Data == null || patch.Operation == JsonPatchOperation.Remove)
                {
                    value = null;
                }
                else
                {
                    try
                    {
                        value = _serializer.Deserialize(patch.Data, valueType);
                    }
                    catch (Exception e)
                    {
                        throw new JsonException("Error in JSON patch", e);
                    }
                }
            }
            else if (endRecursion && target.Type == PathedObjectType.Property)
            {
                var currentPropertyValue = targetProperty.GetValue(value);
                if (patch.Data == null || patch.Operation == JsonPatchOperation.Remove)
                {
                    targetProperty.SetValue(value, null);
                }
                else
                {
                    // Do not use path.Value, it doesn't cast to IDictionary.
                    object deserializedPatch;
                    try
                    {
                        deserializedPatch = _serializer.Deserialize(patch.Data, targetProperty.PropertyType);
                    }
                    catch (Exception e)
                    {
                        throw new JsonException("Error in JSON patch", e);
                    }

                    if (typeof(IDictionary).IsAssignableFrom(targetProperty.PropertyType))
                    {
                        if (currentPropertyValue == null)
                        {
                            currentPropertyValue = Activator.CreateInstance(targetProperty.PropertyType);
                            targetProperty.SetValue(value, currentPropertyValue);
                        }
                        if (patch.Operation == JsonPatchOperation.Replace)
                        {
                            (currentPropertyValue as IDictionary).Clear();
                        }
                        if (patch.Operation == JsonPatchOperation.Add || patch.Operation == JsonPatchOperation.Replace)
                        {
                            UpdateDictionary(currentPropertyValue as IDictionary, deserializedPatch as IDictionary);
                            // No need to do this: target.Property.SetValue(value, currentPropertyValue);
                        }
                        else throw new NotImplementedException(string.Format("Json Patch operation '{0}'", patch.Op));
                    }
                    else if (typeof(IList).IsAssignableFrom(targetProperty.PropertyType)
                        && patch.Operation == JsonPatchOperation.Replace
                        && currentPropertyValue != null 
                        && (currentPropertyValue as IList).Count == 0)
                    {
                        // An empty C# list cannot be set in Firebase, so it's null up there,
                        // and we want to avoid a null received from Firebase to nullify a C# list:
                        // So do not nullify
                    }
                    else
                    {
                        // IList goes also here.
                        if (patch.Operation == JsonPatchOperation.Add || patch.Operation == JsonPatchOperation.Replace)
                        {
                            targetProperty.SetValue(value, deserializedPatch);
                        }
                        else if (patch.Operation == JsonPatchOperation.Remove)
                        {
                            targetProperty.SetValue(value, null);
                        }
                        else throw new NotImplementedException(string.Format("Json Patch operation '{0}'", patch.Op));
                    }
                }
            }
            else if (endRecursion && target.Type == PathedObjectType.DictionaryItem)
            {
                // Should be nicer code, but KeyValuePair needs to know the generic type ahead of time
                //// See http://stackoverflow.com/questions/24589628/reflection-on-idictionary-doesnt-reveal-anything-about-keyvaluepair
                //var typeKeyValue = valueType.GetInterfaces().Single(i => i.Name == "ICollection`1")
                //                        .GetGenericArguments().Single();
                //var jsonKeyValue = string.Format("\"{0}\":{1}", currentPropertyName, patch);
                //var newKeyValue = JsonConvert.DeserializeObject(jsonKeyValue, typeKeyValue, JSON_SETTINGS);
                //(value as IDictionary).Add(newKeyValue as Key);

                string jsonNewDictionary;
                if (patch.Data == null)
                {
                    jsonNewDictionary = "{" + string.Format("\"{0}\":{1}", target.Name, "null") + "}";
                }
                else
                {
                    jsonNewDictionary = "{" + string.Format("\"{0}\":{1}", target.Name, patch.Data) + "}";
                }
                object deserializedDictionary;
                try
                {
                    deserializedDictionary = _serializer.Deserialize(jsonNewDictionary, valueType);
                }
                catch(Exception e)
                {
                    throw new JsonException("Error in dictionary JSON", e);
                }
                if (!(deserializedDictionary is IDictionary))
                {
                    throw new JsonException("Invalid dictionary JSON format.");
                }
                if ((deserializedDictionary as IDictionary).Count != 1)
                {
                    throw new InvalidOperationException("Expecting only 1 keyvalue pair.");
                }
                if (patch.Operation == JsonPatchOperation.Add || patch.Operation == JsonPatchOperation.Replace)
                {
                    // It would be an error for a replace operation here to replace the full dictionary:
                    // indeed, the replacement is only intended for the dictionary item.
                    UpdateDictionary(value as IDictionary, deserializedDictionary as IDictionary);
                }
                else throw new NotImplementedException(string.Format("Json Patch operation '{0}'", patch.Op));
            }
            else if (endRecursion && target.Type == PathedObjectType.ListItem)
            {
                // Should be nicer code, but KeyValuePair needs to know the generic type ahead of time
                //// See http://stackoverflow.com/questions/24589628/reflection-on-idictionary-doesnt-reveal-anything-about-keyvaluepair
                var listInterfaces = valueType.GetInterfaces();
                var typeListItem = listInterfaces.Single(i => i.Name == "ICollection`1").GetGenericArguments().Single();

                int newListItemIndex = 0;
                if (target.Name == "-")
                {
                    if (patch.Operation != JsonPatchOperation.Add)
                    {
                        throw new InvalidOperationException("Expection 'add' operation when appending item with '-' character.");
                    }
                    // RFC6901: The "-" character is used to append to the end of the array.
                    newListItemIndex = (value as IList).Count;
                }
                else if (patch.Operation == JsonPatchOperation.Add 
                    || patch.Operation == JsonPatchOperation.Replace 
                    || patch.Operation == JsonPatchOperation.Remove)
                {
                    newListItemIndex = int.Parse(target.Name);
                }
                else throw new NotImplementedException(string.Format("Json Patch operation '{0}'", patch.Op));
                object deserializedListItem;
                try
                {
                    if (patch.Data == null)
                    {
                        deserializedListItem = _serializer.Deserialize("null", typeListItem);
                    }
                    else
                    {
                        deserializedListItem = _serializer.Deserialize(patch.Data, typeListItem);
                    }
                }
                catch(Exception e)
                {
                    throw new JsonException("Error in collection JSON", e);
                }
                if (patch.Operation == JsonPatchOperation.Replace)
                {
                    if (newListItemIndex + 1 <= (value as IList).Count)
                    {
                        (value as IList)[newListItemIndex] = deserializedListItem;
                    }
                    else if (newListItemIndex + 1 == (value as IList).Count + 1)
                    {
                        (value as IList).Add(deserializedListItem);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(string.Format("Cannot replace item at index {0}.", newListItemIndex));
                    }
                }
                else if (patch.Operation == JsonPatchOperation.Add)
                {
                    if (newListItemIndex + 1 <= (value as IList).Count)
                    {
                        (value as IList).Insert(newListItemIndex, deserializedListItem);
                    }
                    else if (newListItemIndex + 1 == (value as IList).Count + 1)
                    {
                        (value as IList).Add(deserializedListItem);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(string.Format("Cannot add item at index {0}.", newListItemIndex));
                    }
                }
                else if (patch.Operation == JsonPatchOperation.Remove)
                {
                    if (newListItemIndex + 1 <= (value as IList).Count)
                    {
                        (value as IList).RemoveAt(newListItemIndex);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(string.Format("Cannot remove item at index {0}.", newListItemIndex));
                    }
                }
                else throw new NotImplementedException(string.Format("Json Patch operation '{0}'", patch.Op));
            }
            else throw new NotImplementedException();


        }

        #region ----- Helper methods

        private Type GetPropertyType(object obj, Type objType, string propertyName)
        {
            PropertyInfo property = objType.GetProperty(propertyName);
            if (property != null) return property.PropertyType;

            if (typeof(IDictionary).IsAssignableFrom(objType))
            {
                var dictionaryItemTypes = objType.GetGenericArguments();
                return dictionaryItemTypes[1]; // type of dictionary value
            }
            else throw new NotImplementedException();
        }

        private void UpdateDictionary(IDictionary toUpdate, IDictionary updatedValues)
        {
            foreach (var key in updatedValues.Keys)
            {
                if (toUpdate.Contains(key))
                {
                    toUpdate[key] = updatedValues[key];
                }
                else
                {
                    toUpdate.Add(key, updatedValues[key]);
                }
            }
        }

        #endregion


        

    }
}

