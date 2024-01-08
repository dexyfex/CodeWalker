using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Core.Utils.TypeConverters;


public class LinkedListPropertyDescripter<T> : PropertyDescriptor
{
    private readonly LinkedListNode<T> node;

    public LinkedListPropertyDescripter(LinkedListNode<T> node)
        : base(CSharpName(node.Value.GetType()), null)
    {
        this.node = node;
    }

    private static string CSharpName(Type type)
    {
        var sb = new StringBuilder();
        var name = type.Name;
        if (!type.IsGenericType)
            return name;
        sb.Append(name.Substring(0, name.IndexOf('`')));
        sb.Append("<");
        sb.Append(string.Join(", ", type.GetGenericArguments()
                                        .Select(CSharpName)));
        sb.Append(">");
        return sb.ToString();
    }

    public override object GetValue(object component)
    {
        return node.Value;
    }

    public override bool IsReadOnly => true;

    public override string Name => node.Value.ToString();

    public override Type PropertyType => node.Value.GetType();

    public override Type ComponentType => node.List.GetType();

    public override bool ShouldSerializeValue(object component) => false;

    public override bool CanResetValue(object component) => false;

    public override void ResetValue(object component)
    {

    }

    public override void SetValue(object component, object value)
    {

    }
}


public class LinkedListConverter<T> : CollectionConverter
{
    public override bool GetPropertiesSupported(ITypeDescriptorContext context) => true;

    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
    {
        LinkedList<T> list = value as LinkedList<T>;
        if (list == null || list.Count == 0)
            return base.GetProperties(context, value, attributes);

        var items = new PropertyDescriptorCollection(null);

        foreach (var item in list)
        {
            var node = list.Find(item);
            items.Add(new LinkedListPropertyDescripter<T>(node));
        }
        return items;
    }
}


public class ExpandableCollectionPropertyDescriptor : PropertyDescriptor
{
    private IList collection;
    private readonly int _index;

    public ExpandableCollectionPropertyDescriptor(IList coll, int idx)
        : base(GetDisplayName(coll, idx), null)
    {
        collection = coll;
        _index = idx;
    }

    private static string GetDisplayName(IList list, int index)
    {
        return $"[{index}]  " + CSharpName(list[index].GetType());
    }

    private static string CSharpName(Type type)
    {
        var sb = new StringBuilder();
        var name = type.Name;
        if (!type.IsGenericType)
            return name;
        sb.Append(name.Substring(0, name.IndexOf('`')));
        sb.Append("<");
        sb.Append(string.Join(", ", type.GetGenericArguments()
                                        .Select(CSharpName)));
        sb.Append(">");
        return sb.ToString();
    }

    public override bool CanResetValue(object component)
    {
        return true;
    }

    public override Type ComponentType
    {
        get { return this.collection.GetType(); }
    }

    public override object GetValue(object component)
    {
        return collection[_index];
    }

    public override bool IsReadOnly
    {
        get { return false; }
    }

    public override string Name
    {
        get { return _index.ToString(CultureInfo.InvariantCulture); }
    }

    public override Type PropertyType
    {
        get { return collection[_index].GetType(); }
    }

    public override void ResetValue(object component)
    {
    }

    public override bool ShouldSerializeValue(object component)
    {
        return true;
    }

    public override void SetValue(object component, object value)
    {
        collection[_index] = value;
    }
}

public class ListConverter : CollectionConverter
{
    public override bool GetPropertiesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
    {
        IList list = value as IList;
        if (list == null || list.Count == 0)
            return base.GetProperties(context, value, attributes);

        var items = new PropertyDescriptorCollection(null);
        for (int i = 0; i < list.Count; i++)
        {
            object item = list[i];
            items.Add(new ExpandableCollectionPropertyDescriptor(list, i));
        }
        return items;
    }
}