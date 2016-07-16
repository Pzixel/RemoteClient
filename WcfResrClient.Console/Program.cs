using WcfRestClient;
using static System.Console;

namespace WcfResrClient.Console
{
    class Program
    {
        public static void Main()
        {
            var ifoo = FooClient.Processor;
            WriteLine(ifoo.RegisterNode(1, 2, 31, 1, 1).Result);
            ifoo.PostSomething(10).Wait();
        }
    }
}
