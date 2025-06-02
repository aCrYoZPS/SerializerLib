using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;

namespace SerializerLib.Xml;

public static class XmlSerializer
{
    public static string Serialize(object? obj)
    {
        using var writer = new StringWriter();
        using var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Indent = true });
        SerializeObject(xmlWriter, obj);
        xmlWriter.Flush();
        return writer.ToString();
    }

    private static void SerializeDictionary(XmlWriter writer, IDictionary dictionary)
    {
        writer.WriteStartElement("Entries");
        foreach (DictionaryEntry entry in dictionary)
        {
            writer.WriteStartElement("Entry");

            writer.WriteStartElement("Key");
            SerializeObject(writer, entry.Key);
            writer.WriteEndElement();

            writer.WriteStartElement("Value");
            SerializeObject(writer, entry.Value);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void SerializeCollection(XmlWriter writer, IEnumerable collection)
    {
        writer.WriteStartElement("Items");
        foreach (var item in collection)
        {
            writer.WriteStartElement("Item");
            SerializeObject(writer, item);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void SerializeObject(XmlWriter writer, object? obj)
    {
        if (obj == null)
        {
            return;
        }

        var type = obj.GetType();
        switch (obj)
        {
            case IDictionary dictionary:
                SerializeDictionary(writer, dictionary);
                writer.WriteEndElement();
                break;
            case IEnumerable collection and not string:
                SerializeCollection(writer, collection);
                writer.WriteEndElement();
                break;
            default:
            {
                if (!IsSimpleType(type))
                {
                    writer.WriteStartElement(type.Name);
                    foreach (var prop in type.GetProperties())
                    {
                        var value = prop.GetValue(obj);
                        writer.WriteStartElement(prop.Name);
                        if (value == null)
                        {
                            writer.WriteString("");
                        }
                        else if (IsSimpleType(prop.PropertyType))
                        {
                            writer.WriteString(value.ToString());
                        }
                        else
                        {
                            SerializeObject(writer, value);
                        }
                    }

                    writer.WriteEndElement();
                }
                else if (IsSimpleType(type))
                {
                    writer.WriteString(obj.ToString());
                }

                break;
            }
        }
    }

    public static T Deserialize<T>(string xml)
    {
        var doc = XDocument.Parse(xml);
        var root = doc.Root;
        return (T)DeserializeElement(root, typeof(T));
    }

    private static object? DeserializeElement(XElement element, Type targetType)
    {
        if (element.Attribute("nil")?.Value == "true")
        {
            return null;
        }

        if (IsCollection(targetType))
        {
            return DeserializeCollection(element, targetType);
        }

        if (IsDictionary(targetType))
        {
            return DeserializeDictionary(element, targetType);
        }

        var instance = Activator.CreateInstance(targetType);

        foreach (var child in element.Elements())
        {
            var prop = targetType.GetProperty(child.Name.LocalName);
            if (prop != null && prop.CanWrite)
            {
                var value = GetValueFromElement(child, prop.PropertyType);
                prop.SetValue(instance, value);
            }
        }

        return instance;
    }

    private static object? GetValueFromElement(XElement element, Type targetType)
    {
        if (element.Attribute("nil")?.Value == "true")
        {
            return null;
        }

        if (IsSimpleType(targetType))
        {
            if (string.IsNullOrEmpty(element.Value))
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            var converter = TypeDescriptor.GetConverter(targetType);
            return converter.ConvertFromString(element.Value);
        }

        return DeserializeElement(element, targetType);
    }

    private static bool IsCollection(Type type)
    {
        if (type.IsArray) return true;
        return type.IsGenericType && (
            type.GetGenericTypeDefinition() == typeof(List<>) ||
            type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
        );
    }

    private static object DeserializeCollection(XElement element, Type collectionType)
    {
        var elementType = collectionType.IsArray
            ? collectionType.GetElementType()
            : collectionType.GetGenericArguments()[0];

        var itemsElement = element.Element("Items");
        if (itemsElement == null)
        {
            return CreateEmptyCollection(collectionType);
        }

        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType);

        foreach (var itemElement in itemsElement.Elements("Item"))
        {
            var item = GetValueFromElement(itemElement, elementType);
            list.Add(item);
        }

        return collectionType.IsArray
            ? ToArray(list, elementType)
            : Convert.ChangeType(list, collectionType);
    }

    private static object CreateEmptyCollection(Type collectionType)
    {
        return collectionType.IsArray
            ? Array.CreateInstance(collectionType.GetElementType(), 0)
            : Activator.CreateInstance(collectionType);
    }

    private static Array ToArray(IList list, Type elementType)
    {
        var array = Array.CreateInstance(elementType, list.Count);
        list.CopyTo(array, 0);
        return array;
    }

    private static bool IsDictionary(Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }

    private static object? DeserializeDictionary(XElement element, Type dictType)
    {
        var typeArgs = dictType.GetGenericArguments();
        var keyType = typeArgs[0];
        var valueType = typeArgs[1];

        var dict = (IDictionary)Activator.CreateInstance(dictType)!;
        var entriesElement = element.Element("Entries");

        if (entriesElement == null) return dict;

        foreach (var entryElement in entriesElement.Elements("Entry"))
        {
            var keyElement = entryElement.Element("Key");
            var valueElement = entryElement.Element("Value");

            var key = GetValueFromElement(keyElement, keyType);
            var value = GetValueFromElement(valueElement, valueType);

            dict.Add(key, value);
        }

        return dict;
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive
               || type == typeof(string)
               || type == typeof(DateTime)
               || type == typeof(decimal)
               || type == typeof(Guid)
               || type == typeof(TimeSpan);
    }
}