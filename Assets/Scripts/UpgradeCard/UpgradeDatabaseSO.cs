using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeDatabase", menuName = "Upgrades/Database", order = 1)]
public class UpgradeDatabaseSO : ScriptableObject
{
    public List<UpgradeSO> upgrades;
}
