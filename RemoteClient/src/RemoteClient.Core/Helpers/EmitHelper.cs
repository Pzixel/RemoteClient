using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace RemoteClient.Core.Helpers
{
    internal static class EmitHelper
    {
        /// <summary>Creates one constructor for each public constructor in the base class. Each constructor simply
        /// forwards its arguments to the base constructor, and matches the base constructor's signature.
        /// Supports optional values, and custom attributes on constructors and parameters.
        /// Does not support n-ary (variadic) constructors</summary>
        public static void CreatePassThroughConstructors<T>(this TypeBuilder builder)
        {
            foreach (var constructor in typeof(T).GetTypeInfo().DeclaredConstructors)
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length > 0 && parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false))
                {
                    throw new ClientGenerationException("Variadic constructors are not supported");
                }

                var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
                var requiredCustomModifiers = parameters.Select(p => p.GetRequiredCustomModifiers()).ToArray();
                var optionalCustomModifiers = parameters.Select(p => p.GetOptionalCustomModifiers()).ToArray();

                var ctor = builder.DefineConstructor(MethodAttributes.Public, constructor.CallingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
                int iSequence = 1;
                foreach (var parameter in parameters)
                {
                    var parameterBuilder = ctor.DefineParameter(iSequence++, parameter.Attributes, parameter.Name);
                    if (parameter.Attributes.HasFlag(ParameterAttributes.HasDefault))
                    {
                        parameterBuilder.SetConstant(parameter.RawDefaultValue);
                    }

                    foreach (var attribute in BuildCustomAttributes(parameter.CustomAttributes))
                    {
                        parameterBuilder.SetCustomAttribute(attribute);
                    }
                }

                foreach (var attribute in BuildCustomAttributes(constructor.CustomAttributes))
                {
                    ctor.SetCustomAttribute(attribute);
                }

                ctor.GetILGenerator().EmitCallWithParams(constructor, parameters.Length);
            }
        }

        public static AutoPropertyInfo EmitAutoProperty(this TypeBuilder tb, string propertyName, Type propertyType)
        {
            var backingField = tb.DefineField($"<{propertyName}>k__BackingField", propertyType, FieldAttributes.Private);
            var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            var getMethod = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getGenerator = getMethod.GetILGenerator();
            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Ldfld, backingField);
            getGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getMethod);

            var setMethod = tb.DefineMethod("set_" + propertyName, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { propertyType });
            var setGenerator = setMethod.GetILGenerator();
            setGenerator.Emit(OpCodes.Ldarg_0);
            setGenerator.Emit(OpCodes.Ldarg_1);
            setGenerator.Emit(OpCodes.Stfld, backingField);
            setGenerator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setMethod);

            return new AutoPropertyInfo(propertyBuilder, backingField);
        }

        public static void EmitCallWithParams(this ILGenerator generator, MethodInfo method, int paramsCount)
        {
            generator.EmitCallWithParams(il => il.EmitCall(OpCodes.Callvirt, method, null), paramsCount);
        }

        public static void EmitCallWithParams(this ILGenerator generator, ConstructorInfo constructor, int paramsCount)
        {
            generator.EmitCallWithParams(il => il.Emit(OpCodes.Call, constructor), paramsCount + 1); // params + this
        }

        private static void EmitCallWithParams(this ILGenerator generator, Action<ILGenerator> emitCallAction, int paramsCount)
        {
            if (paramsCount > 255)
            {
                throw new ArgumentException("Cannot emit a method call with more than 255 arguments");
            }
            for (int i = 0; i < paramsCount; i++)
            {
                generator.EmitLdarg(i);
            }
            emitCallAction(generator);
            generator.Emit(OpCodes.Ret);
        }

        public static void EmitLdarg(this ILGenerator generator, int i)
        {
            switch (i)
            {
                case 0:
                    generator.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    generator.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    generator.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    generator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    generator.Emit(OpCodes.Ldarg_S, (byte) i);
                    break;
            }
        }

        public static void EmitLdloc(this ILGenerator generator, int i)
        {
            switch (i)
            {
                case 0:
                    generator.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    generator.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    generator.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    generator.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    generator.Emit(OpCodes.Ldloc_S, (byte) i);
                    break;
            }
        }

        private static IEnumerable<CustomAttributeBuilder> BuildCustomAttributes(IEnumerable<CustomAttributeData> customAttributes)
        {
            return customAttributes.Select(attribute =>
            {
                var attributeArgs = attribute.ConstructorArguments.Select(a => a.Value.ResolveValue()).ToArray();

                if (attribute.NamedArguments == null)
                    throw new ArgumentNullException(nameof(attribute), "attribute.NamedArguments is null!");
                var fields = new List<FieldInfo>();
                var fieldValues = new List<object>();
                var properties = new List<PropertyInfo>();
                var propertyValues = new List<object>();

                var allFields = attribute.AttributeType.GetRuntimeFields().ToDictionary(x => x.Name);
                var allProperties = attribute.AttributeType.GetRuntimeProperties().ToDictionary(x => x.Name);

                foreach (var customAttributeNamedArgument in attribute.NamedArguments)
                {
                    if (customAttributeNamedArgument.IsField)
                    {
                        var field = allFields[customAttributeNamedArgument.MemberName];
                        fields.Add(field);
                        fieldValues.Add(customAttributeNamedArgument.TypedValue.Value.ResolveValue());
                    }
                    else
                    {
                        var prop = allProperties[customAttributeNamedArgument.MemberName];
                        properties.Add(prop);
                        propertyValues.Add(customAttributeNamedArgument.TypedValue.Value.ResolveValue());
                    }
                }

                return new CustomAttributeBuilder(attribute.Constructor, attributeArgs, properties.ToArray(), propertyValues.ToArray(), fields.ToArray(), fieldValues.ToArray());
            });
        }

        internal static object ResolveValue(this object value)
        {
            // We are only hunting for the case of the ReadOnlyCollection<T> here.
            var sourceArray = value as ReadOnlyCollection<CustomAttributeTypedArgument>;

            if (sourceArray == null)
            {
                return value;
            }

            if (sourceArray.Count == 0)
            {
                return Array.Empty<object>();
            }

            var underlyingType = sourceArray[0].ArgumentType; // type to be used for arguments
            var listType = typeof(List<>).MakeGenericType(underlyingType);
            var argList = (IList) Activator.CreateInstance(listType);

            foreach (CustomAttributeTypedArgument typedArgument in sourceArray)
            {
                if (underlyingType != typedArgument.ArgumentType)
                {
                    throw new InvalidOperationException("Types for the same named parameter of array type are expected to be same");
                }

                argList.Add(typedArgument.Value);

            }
            var toArrayMethod = listType.GetTypeInfo().GetMethod("ToArray");
            return toArrayMethod.Invoke(argList, Array.Empty<object>());
        }
    }
}
