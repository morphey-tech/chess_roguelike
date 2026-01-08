using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteUI.Binding.Attributes;

namespace LiteUI.Dialog.Converter
{
    public static class DialogParamConverter
    {
        public static object?[]? Convert<T>(List<string?> initParams)
        {
            return Convert(typeof(T), initParams);
        }
        
        public static object?[]? Convert<T>(List<object?> initParams)
        {
            return Convert(typeof(T), initParams);
        }
        
        public static object?[]? Convert(Type dialogType, List<string?> initParams)
        {
            List<Type>? paramTypes = GetParamTypes(dialogType);
            if (paramTypes == null) {
                return null;
            }
            List<object?> castedParams = paramTypes.Select((paramType, i) => ConvertParam(initParams[i], paramType)).ToList();
            return castedParams.Count != 0 ? castedParams.ToArray() : null;
        }
        
        public static object?[]? Convert(Type dialogType, List<object?> initParams)
        {
            List<Type>? paramTypes = GetParamTypes(dialogType);
            if (paramTypes == null) {
                return null;
            }
            List<object?> castedParams = paramTypes.Select((paramType, i) => ConvertParam(initParams[i], paramType)).ToList();
            return castedParams.Count != 0 ? castedParams.ToArray() : null;
        }

        private static List<Type>? GetParamTypes(Type dialogType)
        {
            MethodInfo? createMethod = dialogType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                                 .FirstOrDefault(m => m.GetCustomAttribute<UICreatedAttribute>() != null);
            return createMethod?.GetParameters().Select(p => p.ParameterType).ToList();
        }
        
        private static object? ConvertParam(object? param, Type paramType)
        {
            switch (paramType) {
                case not null when paramType == typeof(int):
                case not null when paramType.IsEnum:
                    return param != null ? System.Convert.ToInt32((double) param) : 0;
                case not null when paramType == typeof(float):
                    return param != null ? (float) param : 0f;
                default:
                    return param;
            }
        }

        private static object? ConvertParam(string? paramValue, Type paramType)
        {
            switch (paramType) {
                case not null when paramType == typeof(string):
                    return paramValue;
                case not null when paramType == typeof(int):
                case not null when paramType.IsEnum:
                    return paramValue != null ? int.Parse(paramValue) : 0;
                case not null when paramType == typeof(float):
                    return paramValue != null ? float.Parse(paramValue) : 0;
                case not null when paramType == typeof(Action):
                case not null when paramType == typeof(Action<>):
                    return null;
                default:
                    throw new NotImplementedException($"Need implement support of param type={paramType!.Name}");
            }
        }
    }
}
