using System;

namespace VFS.VFS.Parser
{
    class Executor : ShellBaseListener
    {
        public override void EnterLs(ShellParser.LsContext context)
        {
            VFSManager.ls(context.files == null ? false : true, context.dirs == null ? false : true);
        }

        public override void EnterCd(ShellParser.CdContext context)
        {
            if (context.path != null)
                VFSManager.cdPath(context.path.Text);
            if (context.ident != null)
                VFSManager.cdIdent(context.ident.Text);
            throw new InvalidArgumentException("cd requires at least one argument");
        }

        public override void EnterCp(ShellParser.CpContext context)
        {
            VFSManager.cp(context.src.Text, context.dst.Text, context.opt == null? false : true);
        }

        public override void EnterCdisk(ShellParser.CdiskContext context)
        {
            base.EnterCdisk(context);
        }

        public override void EnterRmdisk(ShellParser.RmdiskContext context)
        {
            base.EnterRmdisk(context);
        }

        public override void EnterMkdir(ShellParser.MkdirContext context)
        {
            base.EnterMkdir(context);
        }

        public override void EnterRm(ShellParser.RmContext context)
        {
            base.EnterRm(context);
        }

        public override void EnterMv(ShellParser.MvContext context)
        {
            base.EnterMv(context);
        }

        public override void EnterIm(ShellParser.ImContext context)
        {
            base.EnterIm(context);
        }

        public override void EnterEx(ShellParser.ExContext context)
        {
            base.EnterEx(context);
        }

        public override void EnterFree(ShellParser.FreeContext context)
        {
            base.EnterFree(context);
        }

        public override void EnterOcc(ShellParser.OccContext context)
        {
            base.EnterOcc(context);
        }
    }
}
