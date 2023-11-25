using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using CriminalChecker.Configs;
using CriminalChecker.Extensions;
using CriminalChecker.Limits;
using CriminalChecker.Nbt;
using CriminalChecker.Variants;
using Spectre.Console;

namespace CriminalChecker;

public static class CriminalCheckerRunner
{
    private const string ConfigFileName = "config.json";

    private static CancellationTokenSource? cancellationTokenSource;

    public static void Run()
    {
        Console.CancelKeyPress += (sender, args) =>
        {
            cancellationTokenSource?.Cancel();
            args.Cancel = true;
        };

        var deserializer = new YamlDotNet.Serialization.Deserializer();
        var limits = deserializer.Deserialize<LimitsWrapper>(File.ReadAllText("./limits.yaml")).Limits;

        PrintHelloMessage();
        var schematicsPath = GetOrCreateSchematicsPath();
        
        StartAction:
        var startActionChoice = AnsiConsole.Prompt(
            new SelectionPrompt<StartActionVariants>
            {
                Title = "Что хочешь сделать?",
                Converter = action => action.GetEnumDescription()
            }.AddChoices(Enum.GetValues<StartActionVariants>()));

        if (startActionChoice == StartActionVariants.ResetConfigs)
        {
            CreateSchematicsPath();
            goto StartAction;
        }

        if (startActionChoice == StartActionVariants.ClearSchematics)
        {
            foreach (var fileToDelete in new DirectoryInfo(schematicsPath).GetFiles())
            {
                fileToDelete.Delete();
            }
            AnsiConsole.MarkupLine("[green]Схемы успешно удалены[/]");
            goto StartAction;
        }

        var schematicsPaths = GetSchematicsPaths(schematicsPath);

        if (schematicsPaths.Length == 0)
        {
            AnsiConsole.MarkupLine("[red][[Error]] Схемы в папке остутствуют[/]");
            goto StartAction;
        }
        
        var filePath = AnsiConsole.Prompt(
            new SelectionPrompt<string>
            {
                Title = "Какую [green]схему[/] проверить?",
                Converter = GetFilePathConverter(),
                PageSize = int.MaxValue,
                MoreChoicesText = "[gray](Это ещё не всё. Листай ниже...)[/]"
            }.AddChoices(schematicsPaths));

        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLineInterpolated($"[red][[Error]] Не найден файл по пути: [yellow]{filePath}[/][/]");
            goto StartAction;
        }

        var nbtParseResult = NbtParser.FromFile(filePath);

        if (nbtParseResult.Width % 16 != 0 || nbtParseResult.Length % 16 != 0 || nbtParseResult.MinX % 16 != 0 ||
            nbtParseResult.MinZ % 16 != 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[red][[Error]] Схематика создана не по чанкам.[/]");
            goto StartAction;
        }
        
        var schematica = new Schematica.Schematica(nbtParseResult);
        var limitsResult = LimitsChecker.CheckLimits(limits, schematica.GetBlockAmountMapping()).ToList();

        var table = WidgetBuilder.GetSummaryTableFromLimits(limitsResult, Path.GetFileName(filePath));

        AnsiConsole.Write(table);

        var notOkLimits = limitsResult.Where(result => result.LimitResultType != LimitResultType.AllIsOk).ToArray();

        if (notOkLimits.Length == 0)
            goto StartAction;

        ChooseActionLimit:
        var chosenLimitName = SelectLimitName(notOkLimits);
        if (chosenLimitName is null)
            goto StartAction;

        var chosenLimit = notOkLimits.Single(limit => limit.Name == chosenLimitName);

        var limitTable = WidgetBuilder.GetLimitInfoTable(chosenLimit, schematica.MinX, schematica.MinZ);
        var limitInfoPanel = WidgetBuilder.GetLimitInfoPanel(chosenLimit);
        AnsiConsole.Write(limitTable);
        AnsiConsole.Write(limitInfoPanel);

        goto ChooseActionLimit;
    }

    private static string[] GetSchematicsPaths(string schematicsPath)
    {
        return Directory.GetFiles(schematicsPath, "*.schematic", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetCreationTime)
            .ToArray();
    }

    private static Func<string, string> GetFilePathConverter()
    {
        return path => $"{Path.GetFileName(path)} ({File.GetCreationTime(path):dd.MM.yyyy HH:mm})";
    } 

    private static string? SelectLimitName(LimitResult[] limitResults)
    {
        cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        var selectionPrompt = new SelectionPrompt<string>
        {
            Title =
                $"Какой {"огран".WithMarkup(Color.Red)} рассмотреть подробнее? {"(Нажми [yellow]CTRL + C[/], чтобы выйти из схемы)".WithMarkup(Color.Aqua)}"
        };
        selectionPrompt.AddChoices(limitResults.Select(limit => limit.Name));
        try
        {
            var limitName = selectionPrompt.ShowAsync(AnsiConsole.Console, token).GetAwaiter().GetResult();
            return limitName;
        }
        catch (OperationCanceledException)
        {
            var linesCount = limitResults.Length + 2;
            var start = Console.CursorTop - linesCount + 1;
            Console.SetCursorPosition(0, start);

            for (var i = 0; i < linesCount; i++)
                Console.WriteLine(new string(' ', Console.WindowWidth));

            Console.SetCursorPosition(0, start);
            return null;
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }
    }

    private static void PrintHelloMessage()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        var version = fvi.FileVersion!;

        AnsiConsole.Write(
            new FigletText("Made By Kisus").Color(Color.Red));
        AnsiConsole.Write(new FigletText($"Criminal Finder v{version}").Color(Color.Yellow));

        var discord = "@kisus".WithMarkup(Color.Fuchsia);
        var panel = new Panel($"Со всеми ошибками и пожеланиями обращаться в дискорд: {discord}");
        AnsiConsole.Write(panel);
    }

    private static string GetOrCreateSchematicsPath()
    {
        const string configErrorDefaultMessage = "[red][[Error]] С конфигами что-то не так. Сейчас создадим заново.[/]";

        if (File.Exists(ConfigFileName))
        {
            try
            {
                var deserializedConfigs = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigFileName));
                if (deserializedConfigs is not null)
                {
                    var schematicsPath = deserializedConfigs.SchematicsPath;
                    if (Directory.Exists(schematicsPath))
                        return schematicsPath;

                    AnsiConsole.Write(new Markup("Данной директории не существует", new Style(Color.Red)));
                }
                else
                    AnsiConsole.MarkupLine(configErrorDefaultMessage);
            }
            catch (JsonException)
            {
                AnsiConsole.MarkupLine(configErrorDefaultMessage);
            }
        }

        return CreateSchematicsPath();
    }

    private static string CreateSchematicsPath()
    {
        var newSchematicsPath =
            AnsiConsole.Ask<string>($"Укажите путь до {"папки со схемами".WithMarkup(Color.Aqua)}:");
        var config = new Config
        {
            SchematicsPath = newSchematicsPath
        };
        var configFileContent = JsonSerializer.Serialize(config);
        File.WriteAllText(ConfigFileName, configFileContent);

        return GetOrCreateSchematicsPath();
    }
}