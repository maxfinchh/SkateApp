using UnityEngine;

public sealed class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;

    private void Update()
    {
        transform.Rotate(0f, 220f * Time.deltaTime, 0f, Space.World);
    }

    public void Collect(SkateRunnerGameManager gameManager)
    {
        if (gameManager != null)
        {
            gameManager.AddCoins(value);
        }

        if (TryGetComponent(out PooledObject pooledObject))
        {
            pooledObject.Release();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
