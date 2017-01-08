using System;

namespace RemoteClient.Test.TestClasses
{
    [AttributeUsage(AttributeTargets.All)]
    public class ExampleAttribute : Attribute
    {
        public ExampleAttribute(ExampleKind initKind, string[] initStrings)
        {
            Kind = initKind;
            Strings = initStrings;
        }

        public ExampleAttribute(ExampleKind initKind) : this(initKind, null) { }
        public ExampleAttribute() : this(ExampleKind.FirstKind, null) { }

        public ExampleKind Kind { get; }
        public string[] Strings { get; }
        public string Note { get; set; }
        public int[] Numbers { get; set; }
    }
}