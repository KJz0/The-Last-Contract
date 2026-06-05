using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    public abstract void ExecuteEffect(
        Enemy target,
        CardDisplay cardSource);
}