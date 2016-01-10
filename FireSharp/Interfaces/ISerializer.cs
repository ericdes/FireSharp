using System;

namespace FireSharp.Interfaces
{
    public interface ISerializer
    {
        /// <summary>
        /// Method to deserialize from JSON format.
        /// </summary>
        T Deserialize<T>(string json);
        object Deserialize(string json, Type objectType);

        /// <summary>
        /// Method to serialize object to JSON format.
        /// </summary>  
        string Serialize(object value);

        /// <summary>
        /// Method to serialize object to JSON format.
        /// </summary>  
        string Serialize<T>(T value);

        ///// <summary>
        ///// JSON patch method. The target object in paramater gets patched against the JSON patch.
        ///// </summary>
        ///// <typeparam name="T">Type of target object to patch</typeparam>
        //void Patch<T>(T target, IJsonPatch jsonPatch);

    }
}