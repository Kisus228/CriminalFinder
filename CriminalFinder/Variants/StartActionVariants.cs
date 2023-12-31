﻿using System.ComponentModel;

namespace CriminalChecker.Variants;

public enum StartActionVariants
{
    [Description("Выбрать схему")] ChooseSchema,
    [Description("Сбросить конфиги")] ResetConfigs,

    [Description("Очистить папку со схемами")]
    ClearSchematics
}