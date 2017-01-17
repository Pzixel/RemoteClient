using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RemoteClient.Core.Helpers;
using Xunit;

namespace RemoteClient.Test.Core
{
    public class CoreTest
    {
        [Fact]
        public void TestAttributeGeneration()
        {
            var fooAttributes = typeof(Foo).GetTypeInfo().DeclaredConstructors.First().CustomAttributes.ToArray();
            var tb = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("TestAssembly"), AssemblyBuilderAccess.Run)
                .DefineDynamicModule("MainModule")
                .DefineType("TestType");

            tb.CreatePassThroughConstructors<Foo>();

            var typeInfo = tb.CreateTypeInfo();
            var cloneAttributes = typeInfo.DeclaredConstructors.First().CustomAttributes.ToArray();

            Assert.Equal(fooAttributes.Length, cloneAttributes.Length);
            Assert.All(fooAttributes, data =>
            {
                var clone = cloneAttributes.First(x => x.AttributeType == data.AttributeType);
                if (clone.AttributeType == typeof(ExampleAttribute))
                {
                    Assert.Equal((ExampleKind) data.ConstructorArguments[0].Value, (ExampleKind)clone.ConstructorArguments[0].Value);
                    Assert.Equal(data.ConstructorArguments[1].Value.ResolveValue(), clone.ConstructorArguments[1].Value.ResolveValue());

                    Assert.Equal((string)data.NamedArguments[0].TypedValue.Value, (string)clone.NamedArguments[0].TypedValue.Value);
                    Assert.Equal(data.NamedArguments[1].TypedValue.Value.ResolveValue(), clone.NamedArguments[1].TypedValue.Value.ResolveValue());
                }
            });
        }


        [Fact]
        public void ImplementatorTest()
        {
            var impl = ReflectionHelper.GetPropertyInterfaceImplementation<IPoint>();
            var implType = impl.AsType();

            var instance = (IPoint) Activator.CreateInstance(implType, 10, 20);
            var defaultInstance = (IPoint)Activator.CreateInstance(implType);

            Assert.Equal(defaultInstance.X, 0);
            Assert.Equal(defaultInstance.Y, 0);
            Assert.Equal(instance.X, 10);
            Assert.Equal(instance.Y, 20);
        }
    }
}
