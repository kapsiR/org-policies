﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotnetOrg.Ospo
{
    public static class OspoClientFactory
    {
        public static async Task<OspoClient> CreateAsync(string token = null)
        {
            if (string.IsNullOrEmpty(token))
                token = await GetTokenAsync();

            return new OspoClient(token);
        }

        private static async Task<string> GetTokenAsync()
        {
            var tokenFileName = GetTokenFileName();
            if (File.Exists(tokenFileName))
                return File.ReadAllText(tokenFileName).Trim();

            var token = await CreateTokenAsync();
            var tokenFileDirectory = Path.GetDirectoryName(tokenFileName);
            Directory.CreateDirectory(tokenFileDirectory);
            File.WriteAllText(tokenFileName, token);

            return token;
        }

        private static string GetExeName()
        {
            var exePath = Environment.GetCommandLineArgs()[0];
            return Path.GetFileNameWithoutExtension(exePath);
        }

        private static string GetTokenFileName()
        {
            var exePath = Environment.GetCommandLineArgs()[0];
            var fileInfo = FileVersionInfo.GetVersionInfo(exePath);
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fileInfo.CompanyName, fileInfo.ProductName, "ospo-token.txt");
        }

        private static async Task<string> CreateTokenAsync()
        {
            var productName = GetExeName();
            var url = "https://ossmsft.visualstudio.com/_usersSettings/tokens";

            Console.WriteLine($"This is the first time you run {productName}.");
            Console.WriteLine($"{productName} needs to access the Open Source Program Office APIs.");
            Console.WriteLine();
            Console.WriteLine($"Let's log you in so it can create a personal access token.");
            Console.WriteLine();
            Console.WriteLine($"Press any key to navigate to {url}");
            Console.WriteLine($"and create a token with the scope User Profil - Read.");

            Console.ReadKey(true);
            Console.WriteLine();

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });

            while (true)
            {
                Console.Write("Enter token: ");
                var token = ReadPassword();

                var client = new OspoClient(token);

                try
                {
                    await client.GetAsync("dotnet-bot");
                    return token;
                }
                catch (OspoUnauthorizedException)
                {
                    Console.WriteLine($"error: invalid token");
                }
            }
        }

        private static string ReadPassword()
        {
            var pwd = new StringBuilder();
            while (true)
            {
                var i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.Remove(pwd.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000')
                {
                    pwd.Append(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd.ToString();
        }
    }
}
