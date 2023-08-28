using CriminalChecker.Extensions;
using CriminalChecker.Limits;
using Spectre.Console;

namespace CriminalChecker;

public static class WidgetBuilder
{
    public static Table GetSummaryTableFromLimits(IEnumerable<LimitResult> limitsResult, string tableTitle)
    {
        var table = new Table
        {
            Caption = new TableTitle(tableTitle, new Style(Color.Aqua))
        };
        table.AddColumns("Имя лимита", "Ограничение по базе", "Количество на базе", "Ограничение по чанку",
            "Количество в чанках", "Результат проверки");

        foreach (var limitResult in limitsResult)
        {
            var resultColor = GetColorByResultType(limitResult.LimitResultType);
            var chunk =
                $"{string.Join(", ", limitResult.BlockAmountMapping.ChunksAmount.Cast<int>().Where(amount => amount != 0))}";
            table.AddRow(limitResult.Name.WithMarkup(resultColor),
                limitResult.TotalLimit.ToString().WithMarkup(resultColor),
                limitResult.TotalAmount.ToString().WithMarkup(resultColor),
                limitResult.ChunkLimit.ToString().WithMarkup(resultColor),
                chunk.WithMarkup(resultColor),
                limitResult.LimitResultType.GetEnumDescription().WithMarkup(resultColor));
        }

        return table;
    }

    public static Table GetLimitInfoTable(LimitResult limitResult, int minX, int minZ)
    {
        var table = new Table
        {
            Title = new TableTitle(limitResult.Name, new Style(Color.Aqua))
        };
        table.AddColumns("Координаты", "Количество");
        
        var chunkAmounts = limitResult.BlockAmountMapping.ChunksAmount;

        for (var x = 0; x < chunkAmounts.GetLength(0); x++)
        {
            for (var z = 0; z < chunkAmounts.GetLength(1); z++)
            {
                var amount = chunkAmounts[x, z];

                if (amount == 0)
                    continue;

                var chunkX = minX + x * 16;
                var chunkZ = minZ + z * 16;

                table.AddRow($"{chunkX}, {chunkZ}", $"{amount}");
            }
        }

        return table;
    }

    public static Panel GetLimitInfoPanel(LimitResult limitResult)
    {
        var chunkLimitStr = limitResult.ChunkLimit is not null ? limitResult.ChunkLimit.ToString() : "отсутствует";
        var resultColor = GetColorByResultType(limitResult.LimitResultType);
        var limitOwn = limitResult.LimitResultType.GetEnumDescription().WithMarkup(resultColor);
        
        return new Panel(
            $"Сумма: {limitResult.TotalAmount}. Огран по базе: {limitResult.TotalLimit}. Огран по чанку: {chunkLimitStr}. Результат проверки: {limitOwn}");
    }

    private static Color GetColorByResultType(LimitResultType limitResultType)
    {
        var resultColor = limitResultType == LimitResultType.AllIsOk ? Color.Green : Color.Red;
        return resultColor;
    }
}