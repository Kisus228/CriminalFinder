// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using ConsoleTableExt;
using CriminalChecker;
using CriminalChecker.BlockAmountCalculations;
using CriminalChecker.Limits;
using CriminalChecker.Nbt;

var limits = JsonSerializer.Deserialize<LimitsWrapper>(File.ReadAllText("./limits.json"))!.Limits;

Console.WriteLine(GetHelloMessage());

ChooseFile:
Console.WriteLine("Введи путь до файла:");
var filePath = Console.ReadLine()!;
if (!File.Exists(filePath))
{
    Console.WriteLine("Такого файла нет.");
    goto ChooseFile;
}

var nbtParseResult = NbtParser.FromFile(filePath);
var schematica = new Schematica(nbtParseResult);
var limitsResult = LimitsChecker.CheckLimits(limits, schematica.GetBlockAmountMapping()).ToList();

ConsoleTableBuilder
    .From(limitsResult)
    .WithColumn("Имя лимита", "Ограничение по базе", "Количество на базе", "Ограничение по чанку",
        "Количество в чанках", "Результат проверки")
    .WithFormatter(4, GetChunkAmountFormatter())
    .ExportAndWriteLine();

var notOkLimits = limitsResult.Where(result => result.LimitResultType != LimitResultType.AllIsOk).ToArray();

if (notOkLimits.Length == 0)
    goto ChooseFile;

ChooseAction:
Console.WriteLine(GetChooseActionMessage(notOkLimits));

ChooseActionAfterIncorrectInput:
var actionNumberStr = Console.ReadLine();
if (!int.TryParse(actionNumberStr, out var actionNumber) || actionNumber > notOkLimits.Length || actionNumber < 0)
{
    Console.WriteLine("Неправильно введён номер действия");
    goto ChooseActionAfterIncorrectInput;
}

if (actionNumber == 0)
    goto ChooseFile;

var chosenLimit = notOkLimits[actionNumber - 1];
var chunkAmounts = chosenLimit.BlockAmountMapping.ChunksAmount;

for (var x = 0; x < chunkAmounts.GetLength(0); x++)
{
    for (var z = 0; z < chunkAmounts.GetLength(1); z++)
    {
        var amount = chunkAmounts[x, z];
        
        if (amount == 0)
            continue;
        
        var chunkX = schematica.MinX + x * 16;
        var chunkZ = schematica.MinZ + z * 16;

        Console.WriteLine($"Координаты: [{chunkX}, {chunkZ}]. Количество: {amount}");
    }
}

goto ChooseAction;

Func<object, string> GetChunkAmountFormatter()
{
    return obj =>
        $"[{string.Join(", ", ((BlockAmountMapping)obj).ChunksAmount.Cast<int>().Where(amount => amount != 0))}]";
}

string GetChooseActionMessage(LimitResult[] notOkLimits)
{
    var sb = new StringBuilder();
    sb.AppendLine("[0]: Вернуться к выбору файла");

    for (var i = 0; i < notOkLimits.Length; i++)
    {
        var number = i + 1;
        var limitResult = notOkLimits[i];
        sb.AppendLine($"[{number}]: Узнать подробнее о [{limitResult.Name}]");
    }

    return sb.ToString();
}

string GetHelloMessage()
{
    var assembly = Assembly.GetExecutingAssembly();
    var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
    var version = fvi.FileVersion!;
    
    var sb = new StringBuilder();
    sb.AppendLine("Привет! Если читаешь это сообщение, значит я... съел деда.");
    sb.AppendLine($"CriminalChecker v{version}");
    sb.AppendLine("Я - Kisus. Когда-нибудь я напишу здесь что-нибудь умное, ну а пока знай,");
    sb.AppendLine("что это программа написана во благо ТПС юным энтузиастом и ст. модератором 7-го TMR (на момент написания этих строк).");
    return sb.ToString();
}