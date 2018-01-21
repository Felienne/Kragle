namespace Kragle.Preparse
{
    public class PreparseSubOptions : SubOptions
    {
        public override void Run()
        {
            FileStore.Init(Path);
            
            Preparser preparser = new Preparser();
            preparser.PreparseCodeDuplicates();
            preparser.RemoveUnchangedProjects();
        }
    }
}
