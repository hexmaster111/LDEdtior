﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LDEditor;

public static class GridExtensions
{
    public static T Parent<T>(this DependencyObject root) where T : class
    {
        if (root is T)
        {
            return root as T;
        }

        DependencyObject parent = VisualTreeHelper.GetParent(root);
        return parent != null ? parent.Parent<T>() : null;
    }

    public static Point GetColumnRow(this Grid obj, Point relativePoint)
    {
        return new Point(GetColumn(obj, relativePoint.X), GetRow(obj, relativePoint.Y));
    }

    private static int GetRow(Grid obj, double relativeY)
    {
        return GetData(obj.RowDefinitions, relativeY);
    }

    private static int GetColumn(Grid obj, double relativeX)
    {
        return GetData(obj.ColumnDefinitions, relativeX);
    }

    private static int GetData<T>(IEnumerable<T> list, double value) where T : DefinitionBase
    {
        var start = 0.0;
        var result = 0;

        var property = typeof(T).GetProperties().FirstOrDefault(p => p.Name.StartsWith("Actual"));
        if (property == null)
        {
            return result;
        }

        foreach (var definition in list)
        {
            start += (double)property.GetValue(definition);
            if (value < start)
            {
                break;
            }

            result++;
        }

        return result;
    }
}