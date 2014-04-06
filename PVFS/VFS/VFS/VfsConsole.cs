using System;
using System.Linq;

namespace VFS.VFS
{
    public class VfsConsole
    {

        public virtual void ErrorMessage(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
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
            Console.Write("Answer (" + options.Aggregate("", (agg, opt) => agg + ", " + opt).Substring(2) + "): ");
            var opts = options.Select(opt => opt.ToLower()).ToArray();
            string answer;
            do
            {
                answer = Console.ReadLine();
                if (answer != null) answer = answer.ToLower();
            } while (!opts.Contains(answer));
            return opts.IndexOf(answer);
        }

        public virtual string Readline(string message)
        {
            Console.WriteLine(message);
            return Console.ReadLine();
        }
    }
}
