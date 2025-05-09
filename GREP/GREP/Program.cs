using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

static class Grep
{
    // The Main Function
    static async Task Main(string[] args)
    {
        bool continueRunning = true;
        bool argsUsed = false; // NEW: track usage

        while (continueRunning)
        {
            // Default values for search parameters
            bool recursive = false;
            long limit = long.MaxValue;
            string? searchTerm = "";
            string filePattern = "*.txt";
            string directory = Directory.GetCurrentDirectory();

            // Set up cancellation
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Console.Clear();

            if (args.Length == 0 || argsUsed)
            {
                GetUserInput(ref searchTerm, ref filePattern, ref recursive, ref limit, ref directory);
            }
            else 
            {
                ParseArguments(args, ref searchTerm, ref filePattern, ref recursive, ref limit, ref directory);
                argsUsed = true; // NEW: prevent reuse
            }

            if (string.IsNullOrEmpty(searchTerm))
            {
                Console.WriteLine("\nError: Search term is required.");
                break;
            }

            SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            Console.WriteLine("\nPress 'c' to cancel the search anytime...");

            try
            {
                // Get all files first
                var files = Directory.GetFiles(directory, filePattern, searchOption)
                                   .OrderBy(f => f)
                                   .ToList();

                // Create a task for cancellation listener
                var cancelTask = Task.Run(() =>
                {
                    if (Console.ReadKey(true).KeyChar == 'c')
                    {
                        cancellationTokenSource.Cancel();
                        Console.WriteLine("\nCancellation requested.");
                    }
                });

                // Process files in parallel
                await ParallelSearchAsync(files, searchTerm, limit, cancellationToken);

                await cancelTask;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nSearch cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            continueRunning = AskForRetry();
        }

        Console.WriteLine("Exiting... Press Enter to close.");
        Console.ReadLine();
    }

    // Method to perform the actual search asynchronously
    static async Task ParallelSearchAsync(List<string> files, string searchTerm, long limit, CancellationToken cancellationToken)
    {
        long resultCount = 0;
        object lockObj = new object();
        bool limitReached = false;

        await Task.Run(() =>
        {
            Parallel.ForEach(files, new ParallelOptions { CancellationToken = cancellationToken }, file =>
            {
                
                if (cancellationToken.IsCancellationRequested || limitReached)
                    return;

                try
                {
                    foreach (string line in File.ReadLines(file))
                    {
                        if (cancellationToken.IsCancellationRequested || limitReached)
                            break;

                        if (line.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        {
                            lock (lockObj)
                            {
                                // Check limit again inside the lock
                                if (resultCount >= limit)
                                {
                                    limitReached = true;
                                    cancellationToken.ThrowIfCancellationRequested();
                                    return;
                                }

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"{file}: {line}");
                                Console.ResetColor();
                                resultCount++;
                            }
                        }
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Console.WriteLine($"Error processing {file}: {ex.Message}");
                }
            });
        });

        lock (lockObj)
        {
            Console.ForegroundColor = resultCount > 0 ? ConsoleColor.Yellow : ConsoleColor.Red;
            Console.WriteLine($"\n{resultCount} matches found. {(limitReached ? "(Limit reached)" : "")}");
            Console.ResetColor();
        }
    }

    // Function to get user input for search parameters when no arguments are passed
    static void GetUserInput(ref string? searchTerm, ref string filePattern, ref bool recursive, ref long limit, ref string directory)
    {
        Console.WriteLine("No arguments provided. Please enter the following:");

        Console.Write("Enter search term: ");
        searchTerm = Console.ReadLine();

        Console.Write("Enter file pattern (default *.txt): ");
        string? patternInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(patternInput))
            filePattern = patternInput;

        Console.Write("Do you want to search recursively? (y/n): ");
        string? recursiveInput = Console.ReadLine()?.ToLower();
        if (recursiveInput == "y")
            recursive = true;

        Console.Write("Enter the result limit (default: no limit): ");
        string? limitInput = Console.ReadLine();
        if (long.TryParse(limitInput, out long parsedLimit))
            limit = parsedLimit;

        Console.Write("Enter the directory path to search (default current directory): ");
        string? directoryInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(directoryInput) && Directory.Exists(directoryInput))
        {
            directory = directoryInput; 
        }
    }

    // Function to parse arguments passed in the command line
    static void ParseArguments(string[] args, ref string? searchTerm, ref string filePattern, ref bool recursive, ref long limit, ref string directory)
    {
        foreach (string arg in args)
        {
            if (arg.StartsWith("-r"))
                recursive = true; 
            else if (arg.StartsWith("-t:"))
            {
                if (long.TryParse(arg.Substring(3), out long n))
                    limit = n; 
            }
            else if (arg.StartsWith("-d:"))
            {
                string directoryPath = arg.Substring(3); 
                if (Directory.Exists(directoryPath))
                {
                    directory = directoryPath; 
                }
                else
                {
                    Console.WriteLine($"Invalid directory: {directoryPath}. Using current directory.");
                }
            }
            else if (string.IsNullOrEmpty(searchTerm))
                searchTerm = arg; // First argument is search term
            else
                filePattern = arg; // Second argument is file pattern (e.g., *.txt)
        }
    }

    // Function to ask the user if they want to search again or exit
    static bool AskForRetry()
    {
        Console.WriteLine("\nDo you want to search again? (y/n): ");
        string userChoice = Console.ReadLine()?.ToLower();
        return userChoice == "y";
    }
}