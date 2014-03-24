using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFS.VFS
{
    class VfsConsole
    {

        public virtual void Error(string Message)
        {
            Console.WriteLine(Message);
        }

        public virtual void Message(string Message)
        {
            Console.WriteLine(Message);
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
