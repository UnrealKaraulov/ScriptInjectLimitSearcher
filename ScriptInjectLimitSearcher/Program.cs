using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ScriptInjectLimitSearcher
{
    class Program
    {





        static void Main(string[] args)
        {
            Console.WriteLine("Inject limit finder to your script:");
            string scriptpath = Console.ReadLine().Replace("\"", "");
            string[] scriptdata = File.ReadAllLines(scriptpath);
            List<string> scriptdatacleaned = new List<string>();



            string regexgetglobal = @"\s*globals";
            string regexgetlocal = @"\s*local";
            string regexendglobal = @"\s*endglobals";
            bool normalmode = true;

            string regexgetfunction = @"\s*function\s+(\w+)";
            string regexendfunction = @"\s*endfunction";
            string regexgetreturn = @"\s*return";



            string skip1 = @"^\s*$";

            string skip2 = @"^\s*//";


            Console.WriteLine("Type: Normal/Stresstest:");

            string mode = Console.ReadLine();

            if (mode == "normal")
            {
                Console.WriteLine("Selected Normal mode.");
                normalmode = true;
            }
            else if (mode == "stresstest")
            {
                Console.WriteLine("Selected Stresstest mode.");
                normalmode = false;
            }

            List<string> globalslist = new List<string>();
            List<string> functionlist = new List<string>();

            int globalid = 0;

            for (int i = 0; i < scriptdata.Length; i++)
            {
                if (Regex.Match(scriptdata[i], skip1).Success || Regex.Match(scriptdata[i], skip2).Success)
                    continue;


                if (Regex.Match(scriptdata[i], regexgetglobal).Success)
                {
                    i++;
                    for (; i < scriptdata.Length; i++)
                    {
                        if (Regex.Match(scriptdata[i], skip1).Success || Regex.Match(scriptdata[i], skip2).Success)
                            continue;
                        if (!Regex.Match(scriptdata[i], regexendglobal).Success)
                        {
                            globalslist.Add(scriptdata[i]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    continue;
                }

                scriptdatacleaned.Add(scriptdata[i]);
            }


            scriptdata = null;
            scriptdata = scriptdatacleaned.ToArray();
            scriptdatacleaned.Clear();


            bool returnfound = false;

            string currentglobalname = "sdfasdf";


            List<string> functiontester = new List<string>();
            functiontester.Add("function testproblems takes nothing returns nothing");


            for (int i = 0; i < scriptdata.Length; i++)
            {
                if (Regex.Match(scriptdata[i], skip1).Success || Regex.Match(scriptdata[i], skip2).Success)
                    continue;

                Match getfunc = Regex.Match(scriptdata[i], regexgetfunction);

                if (getfunc.Success)
                {
                    functionlist.Add(scriptdata[i]);
                    i++;
                    currentglobalname = "FuncTest" + globalid++;
                    globalslist.Add("integer " + currentglobalname +" = 0");

                    for (; i < scriptdata.Length; i++)
                    {
                        if (Regex.Match(scriptdata[i], regexgetlocal).Success)
                        {
                            functionlist.Add(scriptdata[i]);
                        }
                        else
                            break;
                    }


                    functiontester.Add("if " + currentglobalname + " == 2 then");
                    functiontester.Add("call BJDebugMsg(\"Function " + getfunc.Groups[1].Value + " problem\")");
                    functiontester.Add("endif");

                    functionlist.Add("if " + currentglobalname + " == 1 then");
                    functionlist.Add("set " + currentglobalname + " = 2");
                    functionlist.Add("endif");
                    functionlist.Add("if " + currentglobalname + " == 0 then");
                    functionlist.Add("set " + currentglobalname + " = 1");

                    if (!normalmode)
                    {
                        for (int n = 0; n < 100; n++)
                        {
                            functionlist.Add("set " + currentglobalname + " = 1");
                        }
                    }
                    functionlist.Add("endif");

                    for (; i < scriptdata.Length; i++)
                    {
                        if (Regex.Match(scriptdata[i], skip1).Success || Regex.Match(scriptdata[i], skip2).Success)
                            continue;

                        if (!Regex.Match(scriptdata[i], regexendfunction).Success)
                        {
                            if (Regex.Match(scriptdata[i], regexgetreturn).Success)
                            {
                                returnfound = true;
                                functionlist.Add("set " + currentglobalname + " = 0");
                            }
                            else
                            {
                                returnfound = false;
                            }
                            functionlist.Add(scriptdata[i]);
                        }
                        else
                        {
                            if (!returnfound)
                            {
                                functionlist.Add("if " + currentglobalname + " == 1 then");
                                functionlist.Add("set " + currentglobalname + " = 0");
                                functionlist.Add("endif");
                            }

                            returnfound = true;
                            functionlist.Add(scriptdata[i]);
                            break;
                        }

                    }


                    continue;
                }
                functionlist.Add(scriptdata[i]);
            }

            functiontester.Add("endfunction");


            List<string> outdata = new List<string>();
            outdata.Add("globals");
            foreach (string global in globalslist)
            {
                outdata.Add(global);
            }
            outdata.Add("endglobals");
            foreach (string func in functiontester)
            {
                outdata.Add(func);
            }
            foreach (string func in functionlist)
            {
                outdata.Add(func);
            }
            File.WriteAllLines(scriptpath + "new.j", outdata.ToArray());
            Console.ReadKey();
        }
    }
}
