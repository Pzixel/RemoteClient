namespace RemoteClient.Test.Core
{
    class Foo
    {
        [Example(ExampleKind.ThirdKind, new[] { "line 1", " line 2", "line 3" }, Note = "Hello", Numbers = new[] { 53, 57, 59 })]
        public Foo()
        {

        }
    }
}