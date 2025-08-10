using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEntry
{
    public string cardId;
    public string position;
}

[System.Serializable]
public class DeckJson
{
    public List<CardEntry> cards = new List<CardEntry>();
}

public class DeckManager : MonoBehaviour
{
    public DeckJson deckJson = new DeckJson();

    [Header("Deck Limits")]
    public int buildPointLimit = 20;
    public int attackCardLimit = 20;
    public int locationCardLimit = 10;
    public int maxCardCopies = 2;

    [Header("Current Deck Stats")]
    public int currentBuildPoints = 0;
    public int currentAttackCards = 0;
    public int currentLocationCards = 0;
    public int currentCreatureCards = 0;

    void Update()
    {
        RefreshDeckStats();
    }

    public bool CanAddCard(string cardCode, string position)
    {
        int currentCopies = GetCardCopiesCount(cardCode);
        if (currentCopies >= maxCardCopies)
        {
            return false;
        }

        string cardType = GetCardType(cardCode);

        switch (cardType)
        {
            case "attack":
                int attackBuildPoints = GetAttackBuildPoints(cardCode);
                if (currentBuildPoints + attackBuildPoints > buildPointLimit)
                {
                    return false;
                }

                if (currentAttackCards >= attackCardLimit)
                {
                    return false;
                }
                break;

            case "location":
                if (currentLocationCards >= locationCardLimit)
                {
                    return false;
                }
                break;

            case "mugic":
                int currentMugicCards = GetCardTypeCount("mugic");
                if (currentMugicCards >= currentCreatureCards)
                {
                    return false;
                }
                break;

            case "creature":
            case "battlegear":
                break;
        }

        return true;
    }

    public void InsertCard(string positionGrid, string cardCode)
    {
        deckJson.cards.RemoveAll(c => c.position == positionGrid);
        deckJson.cards.Add(new CardEntry { cardId = cardCode, position = positionGrid });
    }

    public void AddCardToList(string listPosition, string cardCode)
    {
        deckJson.cards.Add(new CardEntry { cardId = cardCode, position = listPosition });
    }

    public void RemoveSlotCard(string positionGrid)
    {
        int removedCount = deckJson.cards.RemoveAll(c => c.position == positionGrid);
    }

    public void RemoveListCard(string listPosition, string cardCode)
    {
        var cardToRemove = deckJson.cards.Find(c =>
            c.position == listPosition && c.cardId == cardCode
        );
        if (cardToRemove != null)
        {
            deckJson.cards.Remove(cardToRemove);
        }
    }

    public void RemoveCard(string positionGrid, string cardCode)
    {
        if (positionGrid.StartsWith("grid-creature") || positionGrid.StartsWith("grid-battlegear"))
        {
            RemoveSlotCard(positionGrid);
        }
        else
        {
            RemoveListCard(positionGrid, cardCode);
        }
    }

    private void RefreshDeckStats()
    {
        currentBuildPoints = CalculateTotalBuildPoints();
        currentAttackCards = GetCardTypeCount("attack");
        currentLocationCards = GetCardTypeCount("location");
        currentCreatureCards = GetCardTypeCount("creature");
    }

    private int CalculateTotalBuildPoints()
    {
        int total = 0;
        foreach (var card in deckJson.cards)
        {
            if (GetCardType(card.cardId) == "attack")
            {
                total += GetAttackBuildPoints(card.cardId);
            }
        }
        return total;
    }

    private int GetCardTypeCount(string cardType)
    {
        int count = 0;
        foreach (var card in deckJson.cards)
        {
            if (GetCardType(card.cardId) == cardType)
                count++;
        }
        return count;
    }

    private int GetCardCopiesCount(string cardCode)
    {
        return deckJson.cards.FindAll(c => c.cardId == cardCode).Count;
    }

    private string GetCardType(string cardCode)
    {
        if (string.IsNullOrEmpty(cardCode))
            return "unknown";

        return cardCode.Split('-')[0];
    }

    private int GetAttackBuildPoints(string cardCode)
    {
        if (GetCardType(cardCode) != "attack")
            return 0;

        string resourcePath = $"Data/Attacks/{cardCode}";
        var attack = Resources.Load<Attack>(resourcePath);

        if (attack == null)
        {
            return 0;
        }

        return attack.buildPoints;
    }

    public int GetCardCountInList(string listPosition, string cardCode)
    {
        return deckJson
            .cards.FindAll(c => c.position == listPosition && c.cardId == cardCode)
            .Count;
    }

    public List<string> GetCardsInList(string listPosition)
    {
        var cardsInList = new List<string>();
        foreach (var card in deckJson.cards)
        {
            if (card.position == listPosition)
            {
                cardsInList.Add(card.cardId);
            }
        }
        return cardsInList;
    }

    public bool IsDeckValid()
    {
        return currentBuildPoints <= buildPointLimit
            && currentAttackCards == attackCardLimit
            && currentLocationCards <= locationCardLimit
            && GetCardTypeCount("mugic") <= currentCreatureCards;
    }

    public string GetDeckValidationMessage()
    {
        List<string> issues = new List<string>();

        if (currentBuildPoints > buildPointLimit)
            issues.Add($"Build Points excedem o limite: {currentBuildPoints}/{buildPointLimit}");

        if (currentAttackCards != attackCardLimit)
            issues.Add(
                $"Deve ter exatamente {attackCardLimit} ataques: {currentAttackCards}/{attackCardLimit}"
            );

        if (currentLocationCards > locationCardLimit)
            issues.Add($"Muitas locations: {currentLocationCards}/{locationCardLimit}");

        int mugicCount = GetCardTypeCount("mugic");
        if (mugicCount > currentCreatureCards)
            issues.Add(
                $"Muitas mugics para a quantidade de criaturas: {mugicCount}/{currentCreatureCards}"
            );

        return issues.Count > 0 ? string.Join("\n", issues) : "Deck válido!";
    }
}
