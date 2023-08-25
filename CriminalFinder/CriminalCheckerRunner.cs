using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using ConsoleTableExt;
using CriminalChecker.BlockAmountCalculations;
using CriminalChecker.Configs;
using CriminalChecker.Limits;
using CriminalChecker.Nbt;

namespace CriminalChecker;

public static class CriminalCheckerRunner
{
    private const string ConfigFileName = "config.json";

    public static void Run()
    {
        var limits = JsonSerializer.Deserialize<LimitsWrapper>(File.ReadAllText("./limits.json"))!.Limits;

        Console.WriteLine(GetHelloMessage());

        var schematicsPath = GetOrCreateSchematicsPath();

        ChooseFile:
        var filePaths = Directory.GetFiles(schematicsPath, "*.schematic", SearchOption.TopDirectoryOnly);

        Space();
        Console.WriteLine("[0]: Сбросить конфиги");
        Console.WriteLine("[1]: Обновить список файлов");
        for (var i = 0; i < filePaths.Length; i++)
        {
            Console.WriteLine($"[{i + 2}]: {Path.GetFileName(filePaths[i])}");
        }

        if (!TryGetActionNumber(filePaths.Length + 1, out var chooseFileActionNumber))
        {
            Console.WriteLine("Неправильно введён номер действия");
            goto ChooseFile;
        }

        switch (chooseFileActionNumber)
        {
            case 0:
                schematicsPath = CreateSchematicsPath();
                goto ChooseFile;
            case 1:
                goto ChooseFile;
        }

        var filePath = filePaths[chooseFileActionNumber - 2];

        var nbtParseResult = NbtParser.FromFile(filePath);
        var schematica = new Schematica(nbtParseResult);
        var limitsResult = LimitsChecker.CheckLimits(limits, schematica.GetBlockAmountMapping()).ToList();

        Space();
        Console.WriteLine(Path.GetFileName(filePath));
        ConsoleTableBuilder
            .From(limitsResult)
            .WithColumn("Имя лимита", "Ограничение по базе", "Количество на базе", "Ограничение по чанку",
                "Количество в чанках", "Результат проверки")
            .WithFormatter(4, GetChunkAmountFormatter())
            .ExportAndWriteLine();

        var notOkLimits = limitsResult.Where(result => result.LimitResultType != LimitResultType.AllIsOk).ToArray();

        if (notOkLimits.Length == 0)
            goto ChooseFile;

        Space();
        Console.WriteLine(GetChooseActionMessage(notOkLimits));

        ChooseActionLimit:
        if (!TryGetActionNumber(notOkLimits.Length, out var chosenLimitNumber))
        {
            Console.WriteLine("Неправильно введён номер действия");
            goto ChooseActionLimit;
        }

        if (chosenLimitNumber == 0)
            goto ChooseFile;

        var chosenLimit = notOkLimits[chosenLimitNumber - 1];
        var chunkAmounts = chosenLimit.BlockAmountMapping.ChunksAmount;

        var chunkLimitStr = chosenLimit.ChunkLimit is not null ? chosenLimit.ChunkLimit.ToString() : "отсутствует";
        Space();
        Console.WriteLine($"[{chosenLimit.Name}]:");
        Console.WriteLine($"Сумма: {chosenLimit.TotalAmount}. Огран по базе: {chosenLimit.TotalLimit}. Огран по чанку: {chunkLimitStr}. Результат проверки: {chosenLimit.LimitResultType}");

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
        Space();

        goto ChooseActionLimit;
    }

    private static void Space()
    {
        Console.WriteLine();
    }

    private static Func<object, string> GetChunkAmountFormatter()
    {
        return obj =>
            $"[{string.Join(", ", ((BlockAmountMapping)obj).ChunksAmount.Cast<int>().Where(amount => amount != 0))}]";
    }

    private static string GetChooseActionMessage(LimitResult[] notOkLimits)
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

    private static string GetHelloMessage()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        var version = fvi.FileVersion!;

        var sb = new StringBuilder();
        sb.AppendLine("Привет! Если читаешь это сообщение, значит я... съел деда.");
        sb.AppendLine($"CriminalChecker v{version}");
        sb.AppendLine("Я - Kisus. Когда-нибудь я напишу здесь что-нибудь умное, ну а пока знай,");
        sb.AppendLine(
            "что это программа написана во благо ТПС юным энтузиастом и ст. модератором 7-го TMR (на момент написания этих строк).");
        return sb.ToString();
    }

    private static string GetOrCreateSchematicsPath()
    {
        const string configErrorDefaultMessage = "[Error] С конфигами что-то не так. Создай их заново.";

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

                    Console.WriteLine("Данной директории не существует");
                }
                else
                    Console.WriteLine(configErrorDefaultMessage);
            }
            catch (JsonException)
            {
                Console.WriteLine(configErrorDefaultMessage);
            }
        }

        return CreateSchematicsPath();
    }

    private static string CreateSchematicsPath()
    {
        Console.WriteLine("Укажите путь до папки со схемами:");
        var newSchematicsPath = Console.ReadLine()!;
        var config = new Config
        {
            SchematicsPath = newSchematicsPath
        };
        var configFileContent = JsonSerializer.Serialize(config);
        File.WriteAllText(ConfigFileName, configFileContent);

        return GetOrCreateSchematicsPath();
    }

    private static bool TryGetActionNumber(int maxNumber, out int actionNumber)
    {
        var actionNumberStr = Console.ReadLine();
        return int.TryParse(actionNumberStr, out actionNumber) && actionNumber <= maxNumber &&
               actionNumber >= 0;
    }
}