using System;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorInterbaseWeb.Pages
{
    public class SelectModel : PageModel
    {
            public IActionResult OnPost()
            {
            string inputDirectory = @"C:\dotnet\wav\"; // Путь к директории с MP3 файлами
            string outputDirectory = @"C:\dotnet\wav1_1\"; // Путь к директории, куда будут сохранены WAV файлы
            string ffmpegExePath = @"C:\dotnet\ffmpeg\ffmpeg.exe"; // Путь к исполняемому файлу FFmpeg

            // Получаем список всех MP3 файлов в директории
            //string[] files = Directory.GetFiles(inputDirectory, "*.mp3");
            string[] files = Directory.GetFiles(inputDirectory);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file); // Получаем имя файла без пути
                string outputFilePath = Path.Combine(outputDirectory, fileName + ".wav"); // Формируем путь к выходному файлу

                // Создаем команду для FFmpeg
                StringBuilder sb = new StringBuilder();
                sb.Append($"{ffmpegExePath} -i ");
                sb.Append(file);
                sb.Append(" -codec:a pcm_alaw -b:a 128k -ar 8000 ");
                sb.Append(outputFilePath);

                // Запускаем FFmpeg с помощью команды
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    using (StreamWriter sw = process.StandardInput)
                    {
                        sw.WriteLine(sb.ToString());
                        sw.WriteLine("exit");
                        sw.Flush();
                    }

                    process.WaitForExit();
                }
            }
        return Page();
        }
    }
}
