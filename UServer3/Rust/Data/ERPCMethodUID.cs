using System;

namespace UServer3.Rust.Data
{
    public enum ERPCMethodUID : UInt32
    {
        OnPlayerLanded =             1998170713, // падение игрока
        FinishLoading =              2811987493,
        StartLoading =               2808526587,
        OnModelState =               1779218792,
        UpdateMetabolism =           858557799,
        BroadcastSignalFromClient =  1552640099,
        PlayerAttack =               4088326849,  // конец атаки инструментами
        CLProject =                  3168282921,  // начало атаки оружием
        OnProjectileAttack =         363681694, // конец атаки оружием
        UpdateLoot =                 1748134015, // с сервера: при перемещении в инвентаре который лутается (открытив в т.ч.)
        MoveItem =                   3041092525, // с клиента: при перемещении
        AddUI =                      804751572,
        DestroyUI =                  3571246553,
        EnterGame =                  1052678473,
        UpdatedItemContainer =       1571447769,
        ForcePositionTo =            3278437942,
        Pickup =                     2778075470, // поднятие ресурсов (гриб...)
        ItemCmd =                    3482449460, // съедание
        UseSelf =                    2918424470, // лечение
        KeepAlive =                  3263238541, // начало поднятия
        Assist =                     970468557,  // само поднятия
        StartReload =                555589155, //начало перезарядки
        Reload =                     1720368164, //конец перезарядки
        ClientKeepConnectionAlive =  935768323, // Desync time
    }
}