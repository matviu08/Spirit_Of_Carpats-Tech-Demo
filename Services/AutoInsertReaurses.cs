using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirit_Of_Carpats_Remake.Services
{
    public class AutoInsertReaurses
    {
        public static void Sync()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo? projectRoot = new DirectoryInfo(baseDir);
            while (projectRoot != null && projectRoot.GetFiles("*.csproj").Length == 0)
            {
                projectRoot = projectRoot.Parent;
            }

            if (projectRoot == null)
            {
                Console.WriteLine("--- [Error] Не вдалося знайти корінь проекту! ---");
                return;
            }

            string sourcePath = Path.Combine(projectRoot.FullName, "Resurses");
            string targetPath = Path.Combine(baseDir, "Resurses");

            Console.WriteLine($"--- Синхронізація ---");
            Console.WriteLine($"Звідки: {sourcePath}");
            Console.WriteLine($"Куди: {targetPath}");

            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine("--- [Warning] Папка Resurses у проекті не знайдена! ---");
                return;
            }

            try
            {
                CopyDirectory(sourcePath, targetPath);
                Console.WriteLine("--- Успішно синхронізовано! ---");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--- [Critical Error] {ex.Message} ---");
            }
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(targetDir, Path.GetFileName(file));

                if (File.Exists(destFile))
                {
                    File.SetAttributes(destFile, FileAttributes.Normal);
                }

                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}
