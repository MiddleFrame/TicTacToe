using System;
using System.Collections.Generic;
using Cards.CustomType;
using Map;

[Serializable]
public class MapData : ISaveble
{
    public List<Road> Roads = new();
    public List<Node> Nodes = new();
}

[Serializable]
public class CardBoolData : ISaveble
{
    public Dictionary<int,bool> BoolData = new();
}

[Serializable]
public class CardListData : ISaveble
{
    public List<int> List = new();
}

[Serializable]
public class SettingsSaveData : ISaveble
{
    public string Language;
    public int CellClearAnimationType;
    public bool IsDevModeEnabled;
}

[Serializable]
public class CoinSaveData : ISaveble
{
    public int CurrentMoney;
}

[Serializable]
public class TutorialSaveData : ISaveble
{
    public bool IsTutorialShowed;
}

[Serializable]
public class StoreSaveData : ISaveble
{
    public bool IsWarningPopupShowed;
}

public interface ISaveble
{
}