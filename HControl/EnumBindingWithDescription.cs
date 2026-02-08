using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;

namespace EnumBindingWithDescription {
    public class EnumDescriptionConverter : IValueConverter {

        object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value == null) return string.Empty;
            Enum myEnum = (Enum)value;
            string description = EnumConverter.GetEnumDescription(myEnum);
            return description;
        }

        object IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return string.Empty;
        }
    }

    public class EnumConverter {

        public static string GetEnumDescription(Enum enumObj) {
            FieldInfo? fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
            object[] attribArray = fieldInfo?.GetCustomAttributes(false) ?? new object[0];

            if (attribArray.Length == 0)
                return enumObj.ToString();
            else {
                DescriptionAttribute? attrib = null;

                foreach (var att in attribArray) {
                    if (att is DescriptionAttribute)
                        attrib = att as DescriptionAttribute;
                }

                if (attrib != null)
                    return attrib.Description;

                return enumObj.ToString();
            }
        }

        public static string GetEnumTooltip(Enum enumObj) {
            FieldInfo? fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
            object[] attribArray = fieldInfo?.GetCustomAttributes(false) ?? new object[0];
            if (attribArray.Length == 0)
                return GetEnumDescription(enumObj);
            else {
                TooltipDescriptionAttribute? attrib = null;
                foreach (var att in attribArray) {
                    if (att is TooltipDescriptionAttribute)
                        attrib = att as TooltipDescriptionAttribute;
                }

                if (attrib != null) return attrib.TooltipDescription;
                return GetEnumDescription(enumObj);
            }
        }
    }

    public class TooltipDescriptionAttribute : Attribute {

        public string TooltipDescription { get; }

        public TooltipDescriptionAttribute(string tooltipDescription) {
            this.TooltipDescription = tooltipDescription;
        }
    }
}