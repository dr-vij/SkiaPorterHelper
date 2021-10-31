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
        private static string mInnerRegexExpression = @"\w+";

        private static string mAddWord = "_renamed";
        private static Regex mRx = new Regex(mRegexExpression, RegexOptions.Compiled);
        private static Regex mInnerRegex = new Regex(mInnerRegexExpression, RegexOptions.Compiled);

        static void Main(string[] args)
        {
            var filesList = new List<string>();
            DirSearch(mPath, filesList);

            //Work with files here
            foreach (var file in filesList)
            {
                var fileStrings = File.ReadAllText(file);
                //we check if we already renamed smth
                var matches = mRx.Matches(fileStrings);

                //takes match and adds a word in the end
                Func<Match, string> renameWord = (Match match) =>
                { return match.Value + mAddWord; };

                //finds words and renames them
                Func<Match, string> findWordAndRename = (Match match) =>
                { return mInnerRegex.Replace(match.Value, new MatchEvaluator(renameWord)); };

                fileStrings = mRx.Replace(fileStrings, new MatchEvaluator(findWordAndRename));
                File.WriteAllText(file, fileStrings);
            }
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
