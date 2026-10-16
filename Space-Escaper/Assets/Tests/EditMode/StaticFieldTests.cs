using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace SpaceEscaper.Tests
{
    public class StaticFieldTests
    {
        private const BindingFlags k_StaticMembers =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        // The editor starts Play Mode without a domain reload, so static fields keep their values
        // from the last Play Mode. Each class empties its own in a method that Unity calls first
        // when Play Mode starts. The test fills every static field of the game, calls these
        // methods like Unity and expects every field to be empty again.
        [Test]
        public void EveryStaticFieldIsResetWhenPlayModeStarts()
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            List<MethodInfo> resets = new List<MethodInfo>();
            FindStaticFieldsAndResets(fields, resets);
            Dictionary<FieldInfo, object> values = SaveValues(fields);
            try
            {
                foreach (FieldInfo field in fields)
                {
                    Fill(field);
                }

                foreach (MethodInfo reset in resets)
                {
                    reset.Invoke(null, null);
                }

                List<string> fieldsNotReset = new List<string>();
                foreach (FieldInfo field in fields)
                {
                    if (!IsEmpty(field))
                    {
                        fieldsNotReset.Add(GetName(field));
                    }
                }

                Assert.That(fieldsNotReset, Is.Empty);
            }
            finally
            {
                // The editor reads them too, the test menu the save for example.
                RestoreValues(values);
            }
        }

        // Constants and readonly values cannot change, readonly lists can. The fields behind
        // events and properties count, the classes the compiler adds for lambdas and coroutines
        // do not.
        private static void FindStaticFieldsAndResets(List<FieldInfo> fields, List<MethodInfo> resets)
        {
            foreach (Type type in typeof(GameManager).Assembly.GetTypes())
            {
                if (type.IsDefined(typeof(CompilerGeneratedAttribute), false))
                {
                    continue;
                }

                foreach (FieldInfo field in type.GetFields(k_StaticMembers))
                {
                    if (!field.IsLiteral && (!field.IsInitOnly || IsList(field.FieldType)))
                    {
                        fields.Add(field);
                    }
                }

                foreach (MethodInfo method in type.GetMethods(k_StaticMembers))
                {
                    RuntimeInitializeOnLoadMethodAttribute attribute =
                        method.GetCustomAttribute<RuntimeInitializeOnLoadMethodAttribute>();
                    if (attribute != null && attribute.loadType == RuntimeInitializeLoadType.SubsystemRegistration)
                    {
                        resets.Add(method);
                    }
                }
            }
        }

        private static bool IsList(Type type)
        {
            return typeof(IList).IsAssignableFrom(type) && !type.IsArray;
        }

        // A readonly list stays the same list, so its items are kept instead.
        private static Dictionary<FieldInfo, object> SaveValues(List<FieldInfo> fields)
        {
            Dictionary<FieldInfo, object> values = new Dictionary<FieldInfo, object>();
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(null);
                values.Add(field, field.IsInitOnly ? new ArrayList((IList)value) : value);
            }

            return values;
        }

        private static void RestoreValues(Dictionary<FieldInfo, object> values)
        {
            foreach (KeyValuePair<FieldInfo, object> pair in values)
            {
                if (!pair.Key.IsInitOnly)
                {
                    pair.Key.SetValue(null, pair.Value);
                    continue;
                }

                IList list = (IList)pair.Key.GetValue(null);
                list.Clear();
                foreach (object item in (ArrayList)pair.Value)
                {
                    list.Add(item);
                }
            }
        }

        // Anything but the value a new domain starts with.
        private static void Fill(FieldInfo field)
        {
            if (field.IsInitOnly)
            {
                ((IList)field.GetValue(null)).Add(null);
            }
            else
            {
                field.SetValue(null, CreateValue(field.FieldType));
            }
        }

        private static object CreateValue(Type type)
        {
            if (type == typeof(bool))
            {
                return true;
            }

            if (type == typeof(string))
            {
                return "From the last Play Mode";
            }

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                return CreateHandler(type);
            }

            Assert.That(type.IsValueType, Is.False, $"The test cannot fill a static {type.Name} yet");

            // Without a constructor, for a component also without a GameObject. That is enough to
            // see whether the field is emptied.
            return FormatterServices.GetUninitializedObject(type);
        }

        // An event handler that does nothing, with the parameters of the event.
        private static Delegate CreateHandler(Type type)
        {
            ParameterInfo[] parameters = type.GetMethod(nameof(Action.Invoke)).GetParameters();
            ParameterExpression[] expressions = new ParameterExpression[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                expressions[i] = Expression.Parameter(parameters[i].ParameterType);
            }

            return Expression.Lambda(type, Expression.Empty(), expressions).Compile();
        }

        // Compared as objects, so a component without a GameObject does not count as empty.
        private static bool IsEmpty(FieldInfo field)
        {
            object value = field.GetValue(null);
            if (field.IsInitOnly)
            {
                return ((IList)value).Count == 0;
            }

            return field.FieldType.IsValueType
                ? value.Equals(Activator.CreateInstance(field.FieldType))
                : value == null;
        }

        // The compiler names the field behind a property like <Instance>k__BackingField.
        private static string GetName(FieldInfo field)
        {
            string name = field.Name;
            if (name[0] == '<')
            {
                name = name.Substring(1, name.IndexOf('>') - 1);
            }

            return field.DeclaringType.Name + "." + name;
        }
    }
}
