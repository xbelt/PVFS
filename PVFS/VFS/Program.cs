using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using VFS.VFS.Parser;

namespace VFS
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var value = Console.ReadLine();
                AntlrInputStream input = new AntlrInputStream(value);
                ShellLexer lexer = new ShellLexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                ShellParser parser = new ShellParser(tokens);

                var entry = parser.compileUnit();

                var walker = new ParseTreeWalker();
                Executor exec = new Executor();
                walker.Walk(exec, entry);
            }
        }
    }
}
