using System;
using System.Linq;

namespace VFS.VFS
{
    public class VfsConsole
    {

        public virtual void Error(string Message)
        {
            Console.WriteLine(Message);
        }

        public virtual void Message(string Message)
        {
            Console.WriteLine(Message);
        }
        
        public virtual void Message(string Message, ConsoleColor textCol)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = textCol;
            Console.WriteLine(Message);
            Console.ForegroundColor = oldColor;
        }

        public virtual int Query(string Message, params string[] options)
        {
            Console.WriteLine(Message);
            string answer;
            do
            {
                answer = Console.ReadLine();
            } while (!options.Contains(answer));
            return options.IndexOf(answer);
        }
    }
}
