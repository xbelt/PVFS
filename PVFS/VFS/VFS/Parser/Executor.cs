using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using VFS.VFS.Models;

namespace VFS.VFS.Parser
{
    class Executor : ShellBaseListener
    {
        public override void EnterLs(ShellParser.LsContext context)
        {
            var entries = VFSManager.ListEntries(context.files == null ? false : true, context.dirs == null ? false : true);
            var oldColor = Console.ForegroundColor;
            foreach (var entry in entries.OrderBy(x => x.IsDirectory))
            {
                if (entry.IsDirectory)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine(entry.Name);
            }
            Console.ForegroundColor = oldColor;
        }

        public override void EnterCd(ShellParser.CdContext context)
        {
            if (context.path != null)
                VFSManager.cdPath(context.path.Text);
            if (context.ident != null)
                VFSManager.ChangeDirectoryByIdentifier(context.ident.Text);
            if (context.dots != null)
                VFSManager.navigateUp();
            throw new InvalidArgumentException("cd requires at least one argument");
        }

        public override void EnterCp(ShellParser.CpContext context)
        {
            VFSManager.cp(context.src.Text, context.dst.Text, context.opt == null? false : true);
        }

        public override void EnterCdisk(ShellParser.CdiskContext context)
        {
            var path = Directory.GetCurrentDirectory();
            var name = "disk" + DateTime.Now + ".vdi";
            name = name.Replace(':', '.');
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

            var disk = DiskFactory.Create(new DiskInfo(path, name, size, blockSize));
            VFSManager.AddAndOpenDisk(disk);
        }

        private double getSizeInBytes(double intSize, string type)
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
            throw new UnsupportedFileSizeType("only kb, mb, gb and tb are allowed as units");
        }

        public override void EnterRmdisk(ShellParser.RmdiskContext context)
        {
            if (context.sys != null && context.sys.Text.EndsWith(".vdi"))
            {
                string path = context.sys.Text;
                var noFileEnding = path.Remove(path.LastIndexOf("."));
                VFSManager.UnloadDisk(noFileEnding.Substring(noFileEnding.LastIndexOf("\\")));
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
                    VFSManager.UnloadDisk(name);
                    name += ".vdi";
                }
                else
                {
                    VFSManager.UnloadDisk(name.Remove(name.LastIndexOf(".")));
                }
                DiskFactory.Remove(path + name);
            }
        }

        public override void EnterLdisk(ShellParser.LdiskContext context)
        {
            VfsDisk disk = null;
            if (context.sys != null && context.sys.Text.EndsWith(".vdi"))
            {
                disk = DiskFactory.Load(context.sys.Text);
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
                disk = DiskFactory.Load(path + name);
            }
            if (disk != null)
            {
                VFSManager.AddAndOpenDisk(disk);
                return;
            }
            throw new DiskNotFoundException();
        }

        public override void EnterLdisks(ShellParser.LdisksContext context)
        {
            IEnumerable<string> files;
            if (context.sys != null)
            {
                files = Directory.EnumerateFiles(context.sys.Text, "*.vdi");
            }
            else
            {
                files = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.vdi");
            }
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
        }

        public override void EnterMkdir(ShellParser.MkdirContext context)
        {
            if (context.id != null)
            {
                EntryFactory.createDirectory(VFSManager.CurrentDisk, context.id.Text, VFSManager.workingDirectory);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void EnterMkFile(ShellParser.MkFileContext context)
        {
            if (context.id != null)
            {
                EntryFactory.createFile(VFSManager.CurrentDisk, context.id.Text, 0, VFSManager.workingDirectory);
            }
            else
            {
                throw new NotImplementedException();
            }
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
            VFSManager.Import(context.ext.Text, context.inte.Text);
        }

        public override void EnterEx(ShellParser.ExContext context)
        {
            VFSManager.Export(context.inte.Text, context.ext.Text);
        }

        public override void EnterFree(ShellParser.FreeContext context)
        {
            base.EnterFree(context);
        }

        public override void EnterOcc(ShellParser.OccContext context)
        {
            base.EnterOcc(context);
        }

        public override void EnterExit(ShellParser.ExitContext context)
        {
            Environment.Exit(0);
        }
    }

    internal class DiskNotFoundException : Exception
    {
    }

    internal class UnsupportedFileSizeType : Exception
    {
        public UnsupportedFileSizeType(string msg)
        {
            throw new NotImplementedException();
        }
    }
}
