using UnityEngine;

[CreateAssetMenu(fileName = "MaskDefinition", menuName = "Maskerpiece/MaskDefinition")]
public class MaskDefinition : ScriptableObject
{
    [SerializeField]
    private Selectable[] selectables;

    public Selectable[] Selectables => selectables;
}
