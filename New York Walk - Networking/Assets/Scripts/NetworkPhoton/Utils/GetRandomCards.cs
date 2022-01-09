using System.Collections.Generic;
using System.Linq;
public class GetRandomCards 
{
    private const int MAX_CARDS_TO_SHARE = 6;
    private const int DECK_COUNT = 24;

    public List<int> GenerateRandom()
    {
        List<int> cardsForBoth = new List<int>();
        for(int i =0; i < MAX_CARDS_TO_SHARE; i++)
        {
            cardsForBoth.Add(UnityEngine.Random.Range(0, DECK_COUNT));
        }
        List<int> result = cardsForBoth.Distinct().ToList();

        while(result.Count < MAX_CARDS_TO_SHARE)
        {
            cardsForBoth.Add(UnityEngine.Random.Range(0, DECK_COUNT));
            result = cardsForBoth.Distinct().ToList();
        }
        return result;
    }
}
