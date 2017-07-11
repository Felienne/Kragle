namespace Kragle.ConsoleOptions
{
    public class ArchiveSubOptions : SubOptions
    {
        public override void Run()
        {
            FileStore.Init(Path);

            new Archiver().Archive();
        }
    }
}
