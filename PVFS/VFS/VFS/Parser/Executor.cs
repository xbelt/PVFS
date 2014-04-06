using System;
using System.Collections.Generic;
using System.IO;
using VFS.VFS.Models;

namespace VFS.VFS.Parser
{
    class Executor : ShellBaseListener
    {
        public override void EnterLs(ShellParser.LsContext context)
        {
            if (context == null)
                return;
            string path = ""; // context.path.Text;
            if (context.files == null && context.dirs == null)
            {
                VfsManager.ListEntries(VfsManager.GetAbsolutePath(path), true, true);
                return;
            }
            VfsManager.ListEntries(VfsManager.GetAbsolutePath(path), context.files == null ? false : true, context.dirs == null ? false : true);
        }

        public override void EnterCd(ShellParser.CdContext context)
        {
            if (context == null)
                return;
            if (context.path != null)
            {
                VfsManager.ChangeWorkingDirectory(context.path.Text);
                return;
            }
            if (context.ident != null)
            {
                VfsManager.ChangeWorkingDirectory(context.ident.Text);
                return;
            }
            if (context.dots != null)
            {
                VfsManager.NavigateUp();
                return;
            }
            throw new ArgumentException("cd requires at least one argument");
        }

        public override void EnterCp(ShellParser.CpContext context)
        {
            if (context == null)
                return;
            VfsManager.Copy(context.src.Text, context.dst.Text);
        }

        public override void EnterCdisk(ShellParser.CdiskContext context)
        {
            if (context == null)
                return;
            var path = Directory.GetCurrentDirectory();
            var name = "disk" + DateTime.Now + ".vdi";
            string pw = "";
            name = name.Replace(':', '_');
            name = name.Replace(' ', '_');
            var blockSize = 2048;
            if (context.par1 != null)
            {
                if (context.par1.path != null)
                {
                    path = context.par1.path.Text;
                }
                if (context.par1.name != null)
                {
                    name = context.par1.name.Text;
                    if (!name.Contains("."))
                    {
                        name += ".vdi";
                    }
                }
                if (context.par1.block != null)
                {
                    blockSize = Convert.ToInt32(context.par1.block.Text);
                }
                if (context.par1.pw != null)
                {
                    pw = context.par1.pw.Text;
                }
            }
            if (context.par2 != null)
            {
                if (context.par2.path != null)
                {
                    path = context.par2.path.Text;
                }
                if (context.par2.name != null)
                {
                    name = context.par2.name.Text;
                    if (!name.EndsWith(".vdi"))
                    {
                        name += ".vdi";
                    }
                }
                if (context.par2.block != null)
                {
                    blockSize = Convert.ToInt32(context.par2.block.Text);
                }
                if (context.par2.pw != null)
                {
                    pw = context.par2.pw.Text;
                }
            }

            var size = 0d;
            if (context.Integer() != null)
            {
                var intSize = (double)Convert.ToInt32(context.Integer().Symbol.Text);
                size = getSizeInBytes(intSize, context.SizeUnit().Symbol.Text);
            }
            if (context.Size() != null)
            {
                var intSize = (double)Convert.ToInt32(context.Size().Symbol.Text.Substring(0, context.Size().Symbol.Text.Length - 2));
                size = getSizeInBytes(intSize, context.Size().Symbol.Text.Substring(context.Size().Symbol.Text.Length - 2));
            }

            VfsManager.CreateDisk(path,name,size,blockSize,pw);
        }

        private static double getSizeInBytes(double intSize, string type)
        {
            switch (type)
            {
                case "kb":
                case "KB":
                    return 1024d * intSize;
                case "mb":
                case "MB":
                    return 1024d * 1024 * intSize;
                case "gb":
                case "GB":
                    return 1024d * 1024 * 1024 * intSize;
                case "tb":
                case "TB":
                    return 1024d * 1024 * 1024 * 1024 * intSize;
            }
            throw new ArgumentException("only kb, mb, gb and tb are allowed as units", "type");
        }

        public override void EnterRmdisk(ShellParser.RmdiskContext context)
        {
            if (context == null)
                return;
            if (context.sys != null && context.sys.Text.EndsWith(".vdi"))
            {
                string path = context.sys.Text;
                var noFileEnding = path.Remove(path.LastIndexOf("."));
                VfsManager.UnloadDisk(noFileEnding.Substring(noFileEnding.LastIndexOf("\\")));
                DiskFactory.Remove(context.sys.Text);
                return;
            }
            if (context.name != null)
            {
                var path = Directory.GetCurrentDirectory();
                if (!path.EndsWith("\\"))
                {
                    path += "\\";
                }
                string name = context.name.Text;
                if (!context.name.Text.EndsWith(".vdi"))
                {
                    VfsManager.UnloadDisk(name);
                    name += ".vdi";
                }
                else
                {
                    VfsManager.UnloadDisk(name.Remove(name.LastIndexOf(".")));
                }
                DiskFactory.Remove(path + name);
            }
        }

        public override void EnterLdisk(ShellParser.LdiskContext context)
        {
            if (context == null)
                return;
            VfsDisk disk = null;
            string pw = "";
            if (context.pw != null)
            {
                pw = context.pw.Text;
            }
            if (context.sys != null && context.sys.Text.EndsWith(".vdi"))
            {
                disk = DiskFactory.Load(context.sys.Text, pw);
            }
            if (context.name != null)
            {
                var path = Directory.GetCurrentDirectory();
                if (!path.EndsWith("\\"))
                {
                    path += "\\";
                }
                string name = context.name.Text;
                if (!context.name.Text.EndsWith(".vdi"))
                    name += ".vdi";
                disk = DiskFactory.Load(path + name, pw);
            }
            if (disk != null)
            {
                VfsManager.LoadDisk(disk);
                return;
            }
        }

        public override void EnterLdisks(ShellParser.LdisksContext context)
        {
            if (context == null)
                return;
            IEnumerable<string> files;
            if (context.sys != null)
            {
                files = Directory.EnumerateFiles(context.sys.Text, "*.vdi");

                foreach (var file in files)
                {
                    VfsManager.Console.Message(file);
                }
            }
            else
            {
                VfsManager.ListDisks();
            }
        }

        public override void EnterMkdir(ShellParser.MkdirContext context)
        {
            if (context == null)
                return;
            if (context.id != null)
            {
                VfsManager.CreateDirectory(VfsManager.GetAbsolutePath(context.id.Text), false);
            }
            else
            {
                VfsManager.Console.Error("Format: mkdir <DirectoryName>");
            }
        }

        public override void EnterMkFile(ShellParser.MkFileContext context)
        {
            if (context == null)
                return;
            if (context.id != null)
            {
                VfsManager.CreateFile(VfsManager.GetAbsolutePath(context.id.Text));
            }
            else
            {
                VfsManager.Console.Error("Format: mk <FileName>");
            }
        }

        public override void EnterRm(ShellParser.RmContext context)
        {
            if (context == null)
                return;
            if (context.trgt != null)
            {
                VfsManager.Remove(context.trgt.Text);
            }
            if (context.id != null)
            {
                VfsManager.Remove(context.id.Text);
            }
        }

        public override void EnterMv(ShellParser.MvContext context)
        {
            if (context == null)
                return;
            VfsManager.Move(context.src.Text, context.dst.Text);
        }

        public override void EnterRn(ShellParser.RnContext context)
        {
            if (context == null)
                return;
            VfsManager.Rename(VfsManager.GetAbsolutePath(context.src.Text), context.dst.Text);
        }

        public override void EnterIm(ShellParser.ImContext context)
        {
            if (context == null)
                return;
            VfsManager.Import(context.ext.Text, context.inte.Text);
        }

        public override void EnterEx(ShellParser.ExContext context)
        {
            if (context == null)
                return;
            VfsManager.Export(context.inte.Text, context.ext.Text);
        }

        public override void EnterFree(ShellParser.FreeContext context)
        {
            if (context == null)
                return;
            VfsManager.GetFreeSpace();
        }

        public override void EnterOcc(ShellParser.OccContext context)
        {
            if (context == null)
                return;
            VfsManager.GetOccupiedSpace();
        }

        public override void EnterHelp(ShellParser.HelpContext context)
        {
            VfsManager.Help();
        }

        public override void EnterDefrag(ShellParser.DefragContext context)
        {
            if (context == null)
                return;
            VfsManager.Defrag();
        }

        public override void EnterExit(ShellParser.ExitContext context)
        {
            if (context == null)
                return;
            VfsManager.Exit();
            Environment.Exit(0);
        }
    }
}
