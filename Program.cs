#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;

namespace gh_pr_helper
{
    class Options
    {
        [Option('r', "repos", Required = false, HelpText = "Repos folder (uses current folder if none is specified)")]
        public string ReposFolder { get; set; }

        [Option('c', "comments-csv", Required = true, HelpText = "A csv file with columns for gh repo folder and the comment")]
        public string CommentsCsv { get; set; }

        [Option('i', "id-column", Required = false, HelpText = "The GitHub account id column name (default: id)")]
        public string IdColumn { get; set; } = "id";

        [Option('f', "feedback-column", Required = false, HelpText = "The feedback / comment column name (default: comment)")]
        public string FeedbackColumn { get; set; } = "comment";

        [Option('l', "list", Required = false, HelpText = "List only what would happen. Does not execute the GH command.")]
        public bool List { get; set; }

    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var result = await CommandLine.Parser.Default.ParseArguments<Options>(args)
              .WithParsedAsync(RunOptions);
            result.WithNotParsed(HandleParseError);
        }

        /// <summary>
        /// The code logic that can use the arguments
        /// </summary>
        /// <param name="opts">The arguments parsed from CLI</param>
        /// <returns></returns>
        static async Task RunOptions(Options opts)
        {
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                Quote = '"'
            };

            if (string.IsNullOrEmpty(opts.ReposFolder))
            {
                opts.ReposFolder = System.IO.Directory.GetCurrentDirectory();
            }

            if (false == System.IO.File.Exists(opts.CommentsCsv))
            {
                Console.WriteLine($"Comments csv file ({opts.CommentsCsv}) does not exist.");
                return;
            }

            // read comments file
            using (var reader = new StreamReader(opts.CommentsCsv))
            {
                int counter = 1;
                await foreach (var line in ReadAsync(reader, config))
                {
                    IDictionary<string, Object> lineData = (IDictionary<string, Object>)line;
                    // process a line from comments csv
                    Console.WriteLine($"{counter++}: {lineData[opts.IdColumn]}: {lineData[opts.FeedbackColumn]}");

                    string path = Path.Combine(opts.ReposFolder, lineData[opts.IdColumn]?.ToString());
                    if (Directory.Exists(path))
                    {
                        if (opts.List)
                        {
                            Console.WriteLine($"\t would run: gh pr comment --body \"{lineData[opts.FeedbackColumn]}\" on path {path}");
                        }
                        else
                        {
                            var p = ExecuteCommand("gh.exe", $"pr comment --body \"{lineData[opts.FeedbackColumn]}\"", path);
                            if (p is not null)
                            {
                                await p.WaitForExitAsync();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Directory for answer repo ({path}) does not exist.");
                    }
                }
            }
        }

        private static async IAsyncEnumerable<dynamic> ReadAsync(StreamReader reader, CsvConfiguration config)
        {
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecordsAsync<dynamic>();
                await foreach (var line in records)
                {
                    yield return line;
                }
            }
        }

        private static Process? ExecuteCommand(string file, string arguments, string workingDirectory)
        {
            // Console.WriteLine($"would execute: {command} in {path}");
            ProcessStartInfo processInfo;

            processInfo = new ProcessStartInfo(file, arguments);
            processInfo.WorkingDirectory = workingDirectory;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = true;

            Process? process = Process.Start(processInfo);
            return process;
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
            Console.WriteLine("Reads comments from the csv file and runs 'gh pr comment --body \"\"' command on each repo. The repos folder must be the base folder for all the repos mentioned in the csv.");
        }
    }
}
