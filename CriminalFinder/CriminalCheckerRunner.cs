﻿using System.Diagnostics;
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

    public static void Run()
    {
        var limits = JsonSerializer.Deserialize<LimitsWrapper>(File.ReadAllText("./limits.json"))!.Limits;

        PrintHelloMessage();

        ChooseFile:
        var schematicsPath = GetOrCreateSchematicsPath();
        var filePaths = Directory.GetFiles(schematicsPath, "*.schematic", SearchOption.TopDirectoryOnly);

        var startActionChoice = AnsiConsole.Prompt(
            new SelectionPrompt<StartActionVariants>
            {
                Title = "Что хочешь сделать?",
                Converter = action => action.GetEnumDescription()
            }.AddChoices(Enum.GetValues<StartActionVariants>()));

        if (startActionChoice == StartActionVariants.ResetConfigs)
        {
            CreateSchematicsPath();
            goto ChooseFile;
        }

        var filePath = AnsiConsole.Prompt(
            new SelectionPrompt<string>
            {
                Title = "Какую [green]схему[/] проверить?",
                Converter = Path.GetFileName,
                WrapAround = true,
                MoreChoicesText = "[gray](Это ещё не всё. Листай ниже...)[/]"
            }.AddChoices(filePaths));

        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLineInterpolated($"[red][[Error]] Не найден файл по пути: [yellow]{filePath}[/][/]");
            goto ChooseFile;
        }

        var nbtParseResult = NbtParser.FromFile(filePath);
        var schematica = new Schematica.Schematica(nbtParseResult);
        var limitsResult = LimitsChecker.CheckLimits(limits, schematica.GetBlockAmountMapping()).ToList();

        var table = WidgetBuilder.GetSummaryTableFromLimits(limitsResult, Path.GetFileName(filePath));

        AnsiConsole.Write(table);

        var notOkLimits = limitsResult.Where(result => result.LimitResultType != LimitResultType.AllIsOk).ToArray();

        if (notOkLimits.Length == 0)
            goto ChooseFile;

        ChooseActionLimit:
        var afterSummaryChoice = AnsiConsole.Prompt(
            new SelectionPrompt<AfterSummaryTableVariants>
            {
                Title = "Куда дальше?",
                Converter = action => action.GetEnumDescription()
            }.AddChoices(Enum.GetValues<AfterSummaryTableVariants>()));

        if (afterSummaryChoice == AfterSummaryTableVariants.ReturnToStart)
            goto ChooseFile;

        var chosenLimitName = AnsiConsole.Prompt(
            new SelectionPrompt<string>
            {
                Title = "Какой [red]огран[/] рассмотреть подробнее?"
            }.AddChoices(notOkLimits.Select(limit => limit.Name)));

        var chosenLimit = notOkLimits.Single(limit => limit.Name == chosenLimitName);

        var limitTable = WidgetBuilder.GetLimitInfoTable(chosenLimit, schematica.MinX, schematica.MinZ);
        var limitInfoPanel = WidgetBuilder.GetLimitInfoPanel(chosenLimit);
        AnsiConsole.Write(limitTable);
        AnsiConsole.Write(limitInfoPanel);

        goto ChooseActionLimit;
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
        var newSchematicsPath = AnsiConsole.Ask<string>($"Укажите путь до {"папки со схемами".WithMarkup(Color.Aqua)}:");
        var config = new Config
        {
            SchematicsPath = newSchematicsPath
        };
        var configFileContent = JsonSerializer.Serialize(config);
        File.WriteAllText(ConfigFileName, configFileContent);

        return GetOrCreateSchematicsPath();
    }
}