using UnityEngine;

namespace cowsins
{
    public class Coin : MonoBehaviour
    {
        [SerializeField] private int minCoins, maxCoins;
        
        [SerializeField] private AudioClip collectCoinSFX; 
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            int amountOfCoins = Random.Range(minCoins, maxCoins); 
            CoinManager.Instance.AddCoins(amountOfCoins);
            UIController.instance.UpdateCoinsPanel(); 
            UIEvents.onCoinsChange?.Invoke(CoinManager.Instance.coins);
            SoundManager.Instance.PlaySound(collectCoinSFX, 0, 1, false, 0); 
            Destroy(this.gameObject); 
        }
    }

}