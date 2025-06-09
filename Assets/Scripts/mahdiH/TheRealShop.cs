using UnityEngine;

public class TheRealShop : MonoBehaviour
{
    [SerializeField]private int coinsCollected = 0;
    [SerializeField]private int diamondsCollected = 0;

    public void AddCoins(int coin)
    {
        coinsCollected += coin;
    }

    public void AddDiamonds(int diamond)
    {
        diamondsCollected += diamond;
    }
}