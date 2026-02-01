using UnityEngine;

[System.Serializable]
public class MaskItem
{
    public Selectable prefab;
}

[CreateAssetMenu(fileName = "MaskDefinition", menuName = "Maskerpiece/MaskDefinition")]
public class MaskDefinition : ScriptableObject
{
    [SerializeField]
    private MaskItem[] masks;

    public MaskItem[] Masks => masks;
}
