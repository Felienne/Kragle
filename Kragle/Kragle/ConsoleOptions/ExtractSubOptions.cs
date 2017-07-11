namespace Kragle.ConsoleOptions
{
    public class ExtractSubOptions : SubOptions
    {
        public override void Run()
        {
            FileStore.Init(Path);

            new Archiver().Extract();
        }
    }
}
