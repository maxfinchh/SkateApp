using UnityEngine;

public sealed class PooledObject : MonoBehaviour
{
    private ObstacleSpawner owner;
    private string poolKey;

    public string PoolKey => poolKey;

    public void ConfigurePool(ObstacleSpawner spawner, string key)
    {
        owner = spawner;
        poolKey = key;
    }

    public void Release()
    {
        if (owner != null)
        {
            owner.ReleaseToPool(this);
            return;
        }

        gameObject.SetActive(false);
    }
}
