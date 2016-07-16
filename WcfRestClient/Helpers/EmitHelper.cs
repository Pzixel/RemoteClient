using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace WcfRestClient.Helpers
{
    public static class EmitHelper
    {
        /// <summary>Creates one constructor for each public constructor in the base class. Each constructor simply
        /// forwards its arguments to the base constructor, and matches the base constructor's signature.
        /// Supports optional values, and custom attributes on constructors and parameters.
        /// Does not support n-ary (variadic) constructors</summary>
        public static void CreatePassThroughConstructors<T>(this TypeBuilder builder)
        {
            foreach (var constructor in typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length > 0 && parameters.Last().IsDefined(typeof(ParamArrayAttribute), false))
                {
                    //Variadic constructors are not supported
                    continue;
                }

                var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
                var requiredCustomModifiers = parameters.Select(p => p.GetRequiredCustomModifiers()).ToArray();
                var optionalCustomModifiers = parameters.Select(p => p.GetOptionalCustomModifiers()).ToArray();

                var ctor = builder.DefineConstructor(MethodAttributes.Public, constructor.CallingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
                for (var i = 0; i < parameters.Length; ++i)
                {
                    var parameter = parameters[i];
                    var parameterBuilder = ctor.DefineParameter(i + 1, parameter.Attributes, parameter.Name);
                    if (parameter.Attributes.HasFlag(ParameterAttributes.HasDefault))
                    {
                        parameterBuilder.SetConstant(parameter.RawDefaultValue);
                    }

                    foreach (var attribute in BuildCustomAttributes(parameter.GetCustomAttributesData()))
                    {
                        parameterBuilder.SetCustomAttribute(attribute);
                    }
                }

                foreach (var attribute in BuildCustomAttributes(constructor.GetCustomAttributesData()))
                {
                    ctor.SetCustomAttribute(attribute);
                }

                ctor.GetILGenerator().EmitCallWithParams(constructor, parameters.Length);
            }
        }

        public static void EmitCallWithParams(this ILGenerator generator, MethodInfo method, int paramsCount)
        {
            generator.EmitCallWithParams(il => il.EmitCall(OpCodes.Call, method, null), paramsCount);
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
            emitCallAction(generator);
            generator.Emit(OpCodes.Ret);
        }

        private static IEnumerable<CustomAttributeBuilder> BuildCustomAttributes(IEnumerable<CustomAttributeData> customAttributes)
        {
            return customAttributes.Select(attribute => {
                var attributeArgs = attribute.ConstructorArguments.Select(a => a.Value).ToArray();
                if (attribute.NamedArguments == null)
                    throw new ArgumentNullException(nameof(attribute), "attribute.NamedArguments is null!");
                var namedPropertyInfos = attribute.NamedArguments.Select(a => a.MemberInfo).OfType<PropertyInfo>().ToArray();
                var namedPropertyValues = attribute.NamedArguments.Where(a => a.MemberInfo is PropertyInfo).Select(a => a.TypedValue.Value).ToArray();
                var namedFieldInfos = attribute.NamedArguments.Select(a => a.MemberInfo).OfType<FieldInfo>().ToArray();
                var namedFieldValues = attribute.NamedArguments.Where(a => a.MemberInfo is FieldInfo).Select(a => a.TypedValue.Value).ToArray();
                return new CustomAttributeBuilder(attribute.Constructor, attributeArgs, namedPropertyInfos, namedPropertyValues, namedFieldInfos, namedFieldValues);
            });
        }
    }
}