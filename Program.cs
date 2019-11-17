namespace Mantis
{
    class Program
    {
        private static void Main(string[] args) => new Mantis().StartAsync().GetAwaiter().GetResult();
    }
}
