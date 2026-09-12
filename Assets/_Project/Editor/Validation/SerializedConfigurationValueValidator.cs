using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.EditorTools.Validation
{
    internal static class SerializedConfigurationValueValidator
    {
        public static void Validate(
            Object configuration,
            string context,
            ICollection<string> errors)
        {
            if (configuration == null)
                return;

            HashSet<object> visited = new(ReferenceComparer.Instance);
            ValidateObject(configuration, context, errors, visited);
        }

        private static void ValidateObject(
            object target,
            string context,
            ICollection<string> errors,
            ISet<object> visited)
        {
            if (target == null || !visited.Add(target))
                return;

            Type type = target.GetType();

            while (type != null && type != typeof(Object) && type != typeof(object))
            {
                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);

                for (int index = 0; index < fields.Length; index++)
                {
                    FieldInfo field = fields[index];

                    if (!IsSerialized(field))
                        continue;

                    ValidateValue(
                        field.GetValue(target),
                        field.FieldType,
                        field,
                        $"{context}: {field.Name}",
                        errors,
                        visited);
                }

                type = type.BaseType;
            }
        }

        private static void ValidateValue(
            object value,
            Type valueType,
            FieldInfo sourceField,
            string context,
            ICollection<string> errors,
            ISet<object> visited)
        {
            if (valueType == typeof(float))
            {
                ValidateFloat((float)value, sourceField, context, errors);
                return;
            }

            if (valueType == typeof(double))
            {
                double number = (double)value;

                if (double.IsNaN(number) || double.IsInfinity(number))
                    errors.Add($"{context} must be finite.");

                return;
            }

            if (valueType == typeof(int))
            {
                ValidateInteger((int)value, sourceField, context, errors);
                return;
            }

            if (valueType.IsEnum)
            {
                if (!Enum.IsDefined(valueType, value))
                    errors.Add($"{context} has unknown value '{value}'.");

                return;
            }

            if (valueType == typeof(Vector2))
            {
                Vector2 vector = (Vector2)value;
                ValidateFinite(vector.x, $"{context}.x", errors);
                ValidateFinite(vector.y, $"{context}.y", errors);
                return;
            }

            if (valueType == typeof(Vector3))
            {
                Vector3 vector = (Vector3)value;
                ValidateFinite(vector.x, $"{context}.x", errors);
                ValidateFinite(vector.y, $"{context}.y", errors);
                ValidateFinite(vector.z, $"{context}.z", errors);
                return;
            }

            if (valueType == typeof(Vector4))
            {
                Vector4 vector = (Vector4)value;
                ValidateFinite(vector.x, $"{context}.x", errors);
                ValidateFinite(vector.y, $"{context}.y", errors);
                ValidateFinite(vector.z, $"{context}.z", errors);
                ValidateFinite(vector.w, $"{context}.w", errors);
                return;
            }

            if (valueType == typeof(Color))
            {
                Color color = (Color)value;
                ValidateFinite(color.r, $"{context}.r", errors);
                ValidateFinite(color.g, $"{context}.g", errors);
                ValidateFinite(color.b, $"{context}.b", errors);
                ValidateFinite(color.a, $"{context}.a", errors);
                return;
            }

            if (valueType == typeof(AnimationCurve))
            {
                ValidateCurve((AnimationCurve)value, context, errors);
                return;
            }

            if (value == null)
            {
                if (typeof(IList).IsAssignableFrom(valueType))
                    errors.Add($"{context} collection is null.");

                return;
            }

            if (value is Object)
                return;

            if (value is IList list)
            {
                for (int index = 0; index < list.Count; index++)
                {
                    object item = list[index];
                    string itemContext = $"{context}[{index}]";

                    if (item == null)
                    {
                        errors.Add($"{itemContext} is null.");
                        continue;
                    }

                    Type itemType = item.GetType();

                    if (ShouldInspectNested(itemType))
                        ValidateObject(item, itemContext, errors, visited);
                }

                return;
            }

            if (ShouldInspectNested(valueType))
                ValidateObject(value, context, errors, visited);
        }

        private static void ValidateFloat(
            float value,
            FieldInfo sourceField,
            string context,
            ICollection<string> errors)
        {
            if (!IsFinite(value))
            {
                errors.Add($"{context} must be finite.");
                return;
            }

            MinAttribute minimum = sourceField.GetCustomAttribute<MinAttribute>();

            if (minimum != null && value < minimum.min)
                errors.Add($"{context} must be at least {minimum.min}.");

            RangeAttribute range = sourceField.GetCustomAttribute<RangeAttribute>();

            if (range != null && (value < range.min || value > range.max))
                errors.Add($"{context} must be in [{range.min}, {range.max}].");
        }

        private static void ValidateInteger(
            int value,
            FieldInfo sourceField,
            string context,
            ICollection<string> errors)
        {
            MinAttribute minimum = sourceField.GetCustomAttribute<MinAttribute>();

            if (minimum != null && value < minimum.min)
                errors.Add($"{context} must be at least {minimum.min}.");

            RangeAttribute range = sourceField.GetCustomAttribute<RangeAttribute>();

            if (range != null && (value < range.min || value > range.max))
                errors.Add($"{context} must be in [{range.min}, {range.max}].");
        }

        private static void ValidateCurve(
            AnimationCurve curve,
            string context,
            ICollection<string> errors)
        {
            if (curve == null || curve.length == 0)
            {
                errors.Add($"{context} must contain at least one key.");
                return;
            }

            Keyframe[] keys = curve.keys;

            for (int index = 0; index < keys.Length; index++)
            {
                ValidateFinite(keys[index].time, $"{context}[{index}].time", errors);
                ValidateFinite(keys[index].value, $"{context}[{index}].value", errors);
                ValidateFinite(keys[index].inWeight, $"{context}[{index}].inWeight", errors);
                ValidateFinite(keys[index].outWeight, $"{context}[{index}].outWeight", errors);
            }
        }

        private static void ValidateFinite(
            float value,
            string context,
            ICollection<string> errors)
        {
            if (!IsFinite(value))
                errors.Add($"{context} must be finite.");
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsSerialized(FieldInfo field)
        {
            if (field.IsStatic || field.IsInitOnly || field.IsNotSerialized)
                return false;

            return field.IsPublic || field.IsDefined(typeof(SerializeField), inherit: true);
        }

        private static bool ShouldInspectNested(Type type)
        {
            return !type.IsPrimitive &&
                   type != typeof(string) &&
                   !typeof(Object).IsAssignableFrom(type) &&
                   type.IsDefined(typeof(SerializableAttribute), inherit: true);
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new();

            public new bool Equals(object left, object right)
            {
                return ReferenceEquals(left, right);
            }

            public int GetHashCode(object value)
            {
                return RuntimeHelpers.GetHashCode(value);
            }
        }
    }
}
