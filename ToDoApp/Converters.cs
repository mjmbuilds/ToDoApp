using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ToDoApp
{
    /// <summary>For Checkbox. 
    /// <para>Returns False if ItemType = Constant, otherwise returns True</para>
    /// </summary>
    class ItemTypeConstantVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((ItemType)value == ItemType.Constant)
            {
                return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>For Icon Once
    /// Returns true if ItemType is Once, otherwise returns False
    /// </summary>
    class ItemTypeOnceVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((ItemType)value == ItemType.Once)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>For Icon Common
    /// Returns true if ItemType is Common, otherwise returns False
    /// </summary>
    class ItemTypeCommonVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((ItemType)value == ItemType.Common)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>For Icon Toggled
    /// Returns true if ItemType is Toggled, otherwise returns False
    /// </summary>
    class ItemTypeToggledVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((ItemType)value == ItemType.Toggled)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
