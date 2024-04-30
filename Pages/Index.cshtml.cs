using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

using InterBaseSql.Data.InterBaseClient;
using RazorInterbaseWeb.Models;

namespace RazorInterbaseWeb.Pages
{
    public class IndexModel : PageModel
    {
        public string Message { get; set; } = "Initial message";
        [BindProperty]
        public InterbaseConnection InterbaseConnection { get; set; } = new InterbaseConnection();

        public IActionResult OnPost()
        {
            Message = "Updated message";

            string connectionString = $"User={InterbaseConnection.User};" +
            $"Password={InterbaseConnection.Password};" +
            $"Database={InterbaseConnection.Database};" +
            $"DataSource={InterbaseConnection.DataSource};" +
            $"Port={InterbaseConnection.Port};";

            string sourceFolderPath = "C:\\dotnet\\wav\\";
            string[] files = Directory.GetFiles(sourceFolderPath);

            using (var connection = new IBConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    Message = $"Error connecting to database: {ex.Message}";
                }

                foreach (var filePath in files) {
                    int key;
                    using (var command = new IBCommand("SELECT * FROM SPR_SPEECH_TABLE WHERE S_INCKEY=(SELECT max(S_INCKEY) FROM SPR_SPEECH_TABLE)", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();
                            key = reader.GetInt32(reader.GetOrdinal("S_INCKEY"));
                        }
                    }
                    key++;


                    // SPR_SPEECH_TABLE  SPR_SP_DATA_1_TABLE
                    using (var transaction = connection.BeginTransaction())
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            byte[] fileData = System.IO.File.ReadAllBytes(path: filePath);

                            // S_DURATION
                            int duration = (int)(fileData.Length / 8000);
                            string durationString = string.Format("{0:D2}:{1:D2}:{2:D2}", duration / 3600, (duration % 3600) / 60, duration % 60);
                          
                            using (var insertCommand = new IBCommand("insert into SPR_SPEECH_TABLE (S_INCKEY, S_TYPE, S_PRELOOKED, S_DATETIME, S_DEVICEID, S_DURATION) values (@S_INCKEY, @S_TYPE, @S_PRELOOKED, @S_DATETIME, @S_DEVICEID, @S_DURATION)", connection, transaction))
                            {
                                DateTime timestampValue = DateTime.Now;

                                insertCommand.Parameters.Add("@S_INCKEY", key);
                                insertCommand.Parameters.AddWithValue("@S_TYPE", 0);
                                insertCommand.Parameters.AddWithValue("@S_PRELOOKED", 0);
                                insertCommand.Parameters.AddWithValue("@S_DATETIME", timestampValue);
                                insertCommand.Parameters.AddWithValue("@S_DURATION", durationString);
                                //insertCommand.Parameters.AddWithValue("@S_EVENTCODE", "G729");
                                insertCommand.Parameters.AddWithValue("@S_DEVICEID", "APK_SUPERACCESS");
                                insertCommand.ExecuteNonQuery();
                            }

                            using (var insertCommand = new IBCommand("insert into SPR_SP_DATA_1_TABLE (S_INCKEY, S_ORDER, S_FSPEECH, S_RECORDTYPE) values (@S_INCKEY, @S_ORDER, @S_FSPEECH, @S_RECORDTYPE)", connection, transaction))
                            {
                                insertCommand.Parameters.Add("@S_INCKEY", key);
                                insertCommand.Parameters.Add("@S_ORDER", 1);
                                insertCommand.Parameters.Add("@S_RECORDTYPE", "PCMA");
                                insertCommand.Parameters.Add("@S_FSPEECH", IBDbType.Array, fileData.Length).Value = fileData;

                                insertCommand.ExecuteNonQuery();
                            }
                        }
                        Timer timer = new Timer((state) =>
                        {
                            Message = "удачно.";
                        }, null, 3000, Timeout.Infinite);
                        transaction.Commit();
                    }
                }
                connection.Close();
                Message = "Connection to InterBase closed.";
            }
            return Page();
        }
    }
}
