using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RegexRenamer
{
    /// <summary>
    /// I use this program to rename methods
    /// It takes .h and .c files and replaces jsimd methods by adding _renamed_by_vij at their end
    /// I was needed this to rename methods to make skiasharp wasm available in unity
    /// </summary>
    class Program
    {
        //path where we are going to fix names
        private static string mPath = "/Users/vij/WorkProjects/ThirdParty/SkiaSharp/externals/skia/third_party/externals/libjpeg-turbo/";
        private static string[] mFileExtensions = new string[] { ".h", ".c" };

        //word starts with jsimd, then words, and then any count of spaces, then ( or ; or )
        private static string mRegexExpression = @"jsimd\w+\s*[(|;|)]";
        private static string mAddWord = "_renamed";
        private static Regex mRx = new Regex(mRegexExpression, RegexOptions.Compiled);

        static void Main(string[] args)
        {
            var filesList = new List<string>();
            DirSearch(mPath, filesList);

            //Work with files here
            foreach (var file in filesList)
            {
                var fileStrings = File.ReadAllLines(file);
                for (int i = 0; i < fileStrings.Length; i++)
                {
                    var str = fileStrings[i];
                    //we check if we already renamed smth
                    if (!str.Contains(mAddWord))
                    {
                        var matches = mRx.Matches(str);
                        foreach (Match match in matches)
                        {
                            string subst = "";
                            if (match.Value.Contains(")") || match.Value.Contains("(") || match.Value.Contains(";"))
                                subst = match.Value.Substring(0, match.Value.Length - 1);
                            else
                                subst = match.Value;

                            str = str.Replace(subst, subst + mAddWord);
                        }
                        if (matches.Count != 0)
                            Console.WriteLine(str);
                        fileStrings[i] = str;
                    }
                }
                File.WriteAllLines(file, fileStrings);
            }

            Console.ReadKey();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="filesList"></param>
        static void DirSearch(string directory, List<string> filesList)
        {
            //Console.WriteLine("DirSearch..(" + sDir + ")");
            try
            {
                //Console.WriteLine("Directory: " + directory);
                var files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    if (mFileExtensions.Length == 0 || mFileExtensions.Any(ext => file.EndsWith(ext)))
                        filesList.Add(file);
                }
                foreach (string dir in Directory.GetDirectories(directory))
                {
                    //exclude git folder
                    if (!dir.Contains(".git"))
                        DirSearch(dir, filesList);
                }
            }
            catch (Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
    }
}
