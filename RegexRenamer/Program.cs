using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RegexRenamer
{
    public class RenameTask
    {
        public string RootPath;
        public string SearchExpression;

        public RenameTask(string rootPath, string searchExpression)
        {
            RootPath = rootPath;
            SearchExpression = searchExpression;
        }

        public string[] FileExtensions = { ".h", ".c" };
        public string AddWord = "_renamed";
        public string WordExpression = @"\w+";
    }

    /// <summary>
    /// I use this program to rename methods
    /// It takes .h and .c files and replaces jsimd methods by adding _renamed_by_vij at their end
    /// I was needed this to rename methods to make skiasharp wasm available in unity
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var renameTasks = new List<RenameTask>();

            //renames all conflicting with Unity funcions of libjpeg
            var path1 = "/Users/vij/WorkProjects/ThirdParty/SkiaSharp/";
            var expression1 = @"\bjsimd\w+\s*([(]|[;]|[)])";
            var task1 = new RenameTask(path1, expression1);
            renameTasks.Add(task1);

            //Renames all conflicting with Unity functions of libpng
            var path2 = "/Users/vij/WorkProjects/ThirdParty/SkiaSharp/";
            var expresstion2 = @"\b(png_get_eXIf|png_handle_eXIf|png_check_chunk_length|png_set_eXIf_1|png_get_uint_32|png_get_int_32|png_get_uint_16|png_zlib_inflate|png_set_eXIf|png_write_eXIf)\b";
            var task2 = new RenameTask(path2, expresstion2);
            task2.FileExtensions = new[] { ".h", ".c", ".cpp" };
            renameTasks.Add(task2);

            //Renames all conflicting with Unity fuction of freetype
            var path3 = "/Users/vij/WorkProjects/ThirdParty/SkiaSharp/";
            var expression3 = @"\b(FT_Get_Multi_Master|FT_Get_MM_Var|FT_Set_MM_Design_Coordinates|FT_Set_Var_Design_Coordinates|FT_Get_Var_Design_Coordinates|FT_Set_MM_Blend_Coordinates|FT_Set_Var_Blend_Coordinates|FT_Get_MM_Blend_Coordinates|FT_Get_Var_Blend_Coordinates)\b";
            var task3 = new RenameTask(path3, expression3);
            task3.FileExtensions = task2.FileExtensions;
            renameTasks.Add(task3);


            foreach (var task in renameTasks)
            {
                var mRx = new Regex(task.SearchExpression);
                var mInnerRegex = new Regex(task.WordExpression);

                var filesList = new List<string>();
                DirSearch(task.RootPath, filesList,task.FileExtensions);

                //Work with files here
                foreach (var file in filesList)
                {
                    var fileStrings = File.ReadAllText(file);
                    //we check if we already renamed smth
                    var matches = mRx.Matches(fileStrings);

                    //rewrite only files with matches
                    if (matches.Count!=0)
                    {
                        //takes match and adds a word in the end
                        Func<Match, string> renameWord = (Match match) =>
                        {
                            if (!match.Value.Contains(task.AddWord))
                                return match.Value + task.AddWord;
                            else
                                return match.Value;
                        };

                        //finds words and renames them
                        Func<Match, string> findWordAndRename = (Match match) =>
                        { return mInnerRegex.Replace(match.Value, new MatchEvaluator(renameWord)); };

                        fileStrings = mRx.Replace(fileStrings, new MatchEvaluator(findWordAndRename));
                        File.WriteAllText(file, fileStrings);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="filesList"></param>
        static void DirSearch(string directory, List<string> filesList, string[] fileExtensions)
        {
            //Console.WriteLine("DirSearch..(" + sDir + ")");
            try
            {
                //Console.WriteLine("Directory: " + directory);
                var files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    if (fileExtensions.Length == 0 || fileExtensions.Any(ext => file.EndsWith(ext)))
                        filesList.Add(file);
                }
                foreach (string dir in Directory.GetDirectories(directory))
                {
                    //exclude git folder
                    if (!dir.Contains(".git"))
                        DirSearch(dir, filesList, fileExtensions);
                }
            }
            catch (Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
    }
}
