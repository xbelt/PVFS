using System;
using System.Linq;

namespace VFS.VFS
{
    public class VfsConsole
    {

        public virtual void ErrorMessage(string message)
        {
            Console.WriteLine(message);
        }

        public virtual void Message(string info)
        {
            Console.WriteLine(info);
        }

        public virtual void Message(string info, ConsoleColor textCol)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = textCol;
            Console.WriteLine(info);
            Console.ForegroundColor = oldColor;
        }

        public virtual int Query(string message, params string[] options)
        {
            Console.WriteLine(message);
            string answer;
            do
            {
                answer = Console.ReadLine();
            } while (!options.Contains(answer));
            return options.IndexOf(answer);
        }

        public virtual string Readline(string message)
        {
            Console.WriteLine(message);
            return Console.ReadLine();
        }
    }
}
