using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using VFS.VFS.Parser;

namespace VFS
{
    class Program
    {
        static void Main()
        {
            while (true)
            {
                var value = Console.ReadLine();
                var input = new AntlrInputStream(value);
                var lexer = new ShellLexer(input);
                var tokens = new CommonTokenStream(lexer);
                var parser = new ShellParser(tokens);

                var entry = parser.compileUnit();

                var walker = new ParseTreeWalker();
                var exec = new Executor();
                walker.Walk(exec, entry);
            }
        }
    }
}
