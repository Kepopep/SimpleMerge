using SimpleMerge.Grid;
using SimpleMerge.Scripts;
using SimpleMerge.Spawning;
using UnityEngine;

public class MergeGridAdapter : MonoBehaviour
{
    [SerializeField] private MergeItemSpawner _spawner;

    public void Execute(MergeItem firsItem, MergeItem secondItem, GridCell cell)
    {
        _spawner.SpawnItemInRandomFreeCell();
    }
}
