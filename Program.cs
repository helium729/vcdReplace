using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ncdReplace
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: vcdReplace.exe <OriginPath> <TargetPath>");
                return;
            }
            string originPath = args[0];
            string targetPath = args[1];
            try
            {
                string currentSession = "";
                StreamReader reader = new StreamReader(originPath);
                StreamWriter writer = new StreamWriter(targetPath);
                string currentLine = "";
                List<Variable> list = new List<Variable>();
                while(!reader.EndOfStream)
                {
                    currentLine = reader.ReadLine();
                    if (currentLine.Length < 2)
                        continue;
                    bool wanted = false;
                    wanted = (currentLine[0] != '$') ||
                        (currentLine.Substring(0, 3) == "$va") ||
                        (currentLine.Substring(0, 3) == "$sc") ||
                        (currentLine.Substring(0, 3) == "$en");
                    if (!wanted)
                        continue;
                    string[] splited = currentLine.Split(' ');
                    switch(splited[0])
                    {
                        case "$end":
                            currentSession = "";
                            break;
                        case "$enddefinitions":
                            currentSession = "";
                            break;
                        case "$scope":
                            currentSession = splited[2];
                            break;
                        case "$var":
                            Variable v = new Variable
                            {
                                Scope = currentSession,
                                Type = splited[1],
                                Alias = splited[3]
                            };
                            if (splited[5] == "$end")
                                v.Name = splited[4];
                            else
                                v.Name = splited[4] + splited[5];
                            list.Add(v);
                            break;
                        default:
                            string t = currentLine;
                            if (splited[0][0] == 'b')
                            {
                                var r = from a in list
                                        where a.Type == "reg"
                                        where a.Alias == splited[1]
                                        select a.FullName;
                                if (r.Count() == 0)
                                    break;
                                string toReplace = r.ElementAt(0);
                                t = splited[0] + toReplace;
                            }
                            else if (splited[0][0] == '#')
                                t = currentLine;
                            else
                            {
                                var r = from a in list
                                        where a.Type == "wire"
                                        where a.Alias == currentLine.Substring(1)
                                        select a.FullName;
                                if (r.Count() == 0)
                                    break;
                                string toReplace = r.ElementAt(0);
                                t = currentLine.Substring(0, 1) + " " + toReplace;
                            }
                            writer.WriteLine(t);
                            break;
                    }//end switch
                }//end while
                reader.Close();
                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }

    class Variable
    {
        private string scope;
        private string name;
        private string alias;

        private string type;

        public string Alias { get => alias; set => alias = value; }
        internal string Scope { get => scope; set => scope = value; }
        internal string Name { get => name; set => name = value; }
        internal string FullName
        {
            set
            {
                string[] splited = value.Split('.');
                if (splited.Length >= 2)
                {
                    scope = splited[0];
                    name = splited[1];
                }
            }
            get => (scope + "." + name);
        }

        internal string Type { get => type; set => type = value; }
    }
}
