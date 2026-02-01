using UnityEngine;

[CreateAssetMenu(fileName = "MaskDefinition", menuName = "Maskerpiece/MaskDefinition")]
public class MaskDefinition : ScriptableObject
{
    [SerializeField]
    private Selectable[] masks;

    public Selectable[] Masks => masks;
}
