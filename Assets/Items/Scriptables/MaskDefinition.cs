using UnityEngine;

[System.Serializable]
public class MaskItem
{
    public Selectable prefab;
    public int price;
}

[CreateAssetMenu(fileName = "MaskDefinition", menuName = "Maskerpiece/MaskDefinition")]
public class MaskDefinition : ScriptableObject
{
    [SerializeField]
    private MaskItem[] masks;

    public MaskItem[] Masks => masks;
}
