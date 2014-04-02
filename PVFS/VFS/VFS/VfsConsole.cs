using System;
using System.Linq;

namespace VFS.VFS
{
    public class VfsConsole
    {

        public virtual void Error(string message)
        {
            Console.WriteLine(message);
        }

        public virtual void Message(string message)
        {
            Console.WriteLine(message);
        }
        
        public virtual void Message(string message, ConsoleColor textCol)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = textCol;
            Console.WriteLine(message);
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
    }
}
