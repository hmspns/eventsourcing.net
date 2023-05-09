using System.Reflection;

namespace EventSourcing.Net.Tests.Shared;

public static class ReflectionExtensions
{
    /// <summary>
    /// Get value of the private property from specific object.
    /// </summary>
    /// <param name="source">Object to get property value</param>
    /// <param name="propertyName">Name of property.</param>
    /// <typeparam name="T">Type of property value.</typeparam>
    /// <returns>Value of the property.</returns>
    /// <exception cref="ArgumentNullException"><param name="source" /> or <param name="propertyName" /> is null.</exception>
    /// <exception cref="ArgumentException">Property has no getter.</exception>
    public static T GetPrivateProperty<T>(this object source, string propertyName)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }
        
        Type t = source.GetType();
        PropertyInfo? property = t.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

        if (property == null)
        {
            throw new ArgumentException($"Property {propertyName} wasn't found");
        }
        if (property.GetMethod == null)
        {
            throw new ArgumentException($"Property {propertyName} has no getter");
        }

        object? value = property.GetValue(source);
        return (T)value;
    }
    
    /// <summary>
    /// Get value of the private field from specific object.
    /// </summary>
    /// <param name="source">Object to get field value</param>
    /// <param name="fieldName">Name of field.</param>
    /// <typeparam name="T">Type of field value.</typeparam>
    /// <returns>Value of the property.</returns>
    /// <exception cref="ArgumentNullException"><param name="source" /> or <param name="fieldName" /> is null.</exception>
    public static T GetPrivateField<T>(this object source, string fieldName)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (fieldName == null)
        {
            throw new ArgumentNullException(nameof(fieldName));
        }
        
        Type t = source.GetType();
        FieldInfo? field = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        if (field == null)
        {
            throw new ArgumentException($"Field {field} wasn't found");
        }
        
        object? value = field.GetValue(source);
        return (T)value;
    }
}