using System;

namespace VFS.VFS.Parser
{
    class Executor : ShellBaseListener
    {
        public override void EnterLs(ShellParser.LsContext context)
        {
            VFSManager.ls();
        }

        public override void EnterCd(ShellParser.CdContext context)
        {
            VFSManager.cd(context.Path().Symbol.Text);
        }
    }
}
