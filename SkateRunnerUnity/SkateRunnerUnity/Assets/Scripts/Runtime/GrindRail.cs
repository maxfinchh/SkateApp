using System.Collections.Generic;
using UnityEngine;

public sealed class GrindRail : MonoBehaviour
{
    private static readonly List<GrindRail> activeRails = new();

    [SerializeField] private float grindHeight = 1.15f;

    public static IReadOnlyList<GrindRail> ActiveRails => activeRails;
    public float GrindHeight => grindHeight;

    private void OnEnable()
    {
        if (!activeRails.Contains(this))
        {
            activeRails.Add(this);
        }
    }

    private void OnDisable()
    {
        activeRails.Remove(this);
    }
}
