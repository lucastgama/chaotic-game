using UnityEngine;
using UnityEngine.EventSystems;

public class CardInspectorTrigger : MonoBehaviour, IPointerClickHandler
{
    private ScriptableObject cardAsset;

    void Awake() { }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CardViewerBuild view = GetComponent<CardViewerBuild>();
            if (view != null)
            {
                cardAsset = view.cardAsset;
            }

            ShowCardData();
        }
    }

    void ShowCardData()
    {
        if (cardAsset == null)
        {
            Debug.LogWarning("NENHUM ASSET CONFIGURADO AUTOMATICAMENTE!");
            return;
        }

        if (cardAsset is Creature creature)
        {
            Debug.Log(
                $"CREATURE: {creature.cardName} | Courage: {creature.courage} | Power: {creature.power} | Wisdom: {creature.wisdom} | Speed: {creature.speed} | Energy: {creature.energy}"
            );
        }
        else if (cardAsset is Attack attack)
        {
            Debug.Log(
                $"ATTACK: {attack.cardName} | Base Damage: {attack.baseDamage} | Build Points: {attack.buildPoints} | Fire: {attack.fireDamage} | Air: {attack.airDamage} | Earth: {attack.earthDamage} | Water: {attack.waterDamage}"
            );
        }
        else if (cardAsset is Battlegear battlegear)
        {
            Debug.Log(
                $"BATTLEGEAR: {battlegear.cardName} | Unique: {battlegear.isUnique} | Loyal: {battlegear.isLoyal} | Legendary: {battlegear.isLegendary}"
            );
        }
        else if (cardAsset is Location location)
        {
            Debug.Log(
                $"LOCATION: {location.cardName} | Initiative: {location.initiative} | Unique: {location.isUnique}"
            );
        }
        else if (cardAsset is Mugic mugic)
        {
            Debug.Log(
                $"MUGIC: {mugic.cardName} | Cost: {mugic.cost} | Tribe: {mugic.tribe} | Unique: {mugic.isUnique}"
            );
        }
        else
        {
            Debug.Log($"TIPO DE CARTA DESCONHECIDO: {cardAsset.GetType().Name}");
        }
    }
}
