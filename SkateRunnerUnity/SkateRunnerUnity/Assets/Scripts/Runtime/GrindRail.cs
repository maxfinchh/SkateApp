using System.Collections.Generic;
using UnityEngine;

public sealed class GrindRail : MonoBehaviour
{
    private static readonly List<GrindRail> activeRails = new();

    private const string EntryKickerName = "Rail Entry Kicker";
    private const string RailBodyName = "Rail Body";

    [SerializeField] private float entryKickerLaunchVelocity = 4.25f;
    [SerializeField] private float entryKickerWindowSeconds = 0.9f;
    [SerializeField] private int entryKickerBonusPoints = 180;
    [SerializeField] private Vector3 entryKickerWorldSize = new(1.65f, 0.35f, 2.5f);
    [SerializeField] private float entryKickerGap = 3.25f;
    [SerializeField] private float entryKickerPitch = -16f;
    [SerializeField] private float entryKickerTopOffset = -0.47f;

    public static IReadOnlyList<GrindRail> ActiveRails => activeRails;
    public float SurfaceHeight
    {
        get
        {
            Transform body = transform.Find(RailBodyName);
            if (body != null && body.TryGetComponent(out Renderer bodyRenderer))
            {
                return bodyRenderer.bounds.max.y;
            }

            return transform.position.y;
        }
    }
    public float EndZ
    {
        get
        {
            Transform body = transform.Find(RailBodyName);
            if (body != null && body.TryGetComponent(out Renderer bodyRenderer))
            {
                return bodyRenderer.bounds.max.z;
            }

            return transform.position.z;
        }
    }

    public float EntryKickerLaunchVelocity => entryKickerLaunchVelocity;
    public float EntryKickerWindowSeconds => entryKickerWindowSeconds;
    public int EntryKickerBonusPoints => entryKickerBonusPoints;

    public void ConfigureEntryKicker(float launchVelocity, float windowSeconds, int bonusPoints)
    {
        entryKickerLaunchVelocity = launchVelocity;
        entryKickerWindowSeconds = windowSeconds;
        entryKickerBonusPoints = bonusPoints;
        EnsureEntryKicker();
    }

    private void OnEnable()
    {
        EnsureEntryKicker();

        if (!activeRails.Contains(this))
        {
            activeRails.Add(this);
        }
    }

    private void OnDisable()
    {
        activeRails.Remove(this);
    }

    private void EnsureEntryKicker()
    {
        Transform kicker = transform.Find(EntryKickerName);
        if (kicker == null)
        {
            GameObject kickerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kickerObject.name = EntryKickerName;
            kickerObject.transform.SetParent(transform, false);
            ConfigureKickerTrigger(kickerObject.GetComponent<BoxCollider>());
            kickerObject.AddComponent<RailEntryKicker>().Configure(this);
            kicker = kickerObject.transform;
        }
        else
        {
            RailEntryKicker entry = kicker.GetComponent<RailEntryKicker>();
            if (entry == null)
            {
                entry = kicker.gameObject.AddComponent<RailEntryKicker>();
            }

            entry.Configure(this);
            ConfigureKickerTrigger(kicker.GetComponent<BoxCollider>());
        }

        Transform railBody = transform.Find(RailBodyName);
        Vector3 bodyPosition = railBody != null ? railBody.localPosition : Vector3.zero;
        Vector3 bodyScale = railBody != null ? railBody.localScale : transform.localScale;
        float railFrontZ = bodyPosition.z - Mathf.Abs(bodyScale.z) * 0.5f;

        kicker.localPosition = new Vector3(
            bodyPosition.x,
            bodyPosition.y + entryKickerTopOffset,
            railFrontZ - entryKickerGap - entryKickerWorldSize.z * 0.5f);
        kicker.localRotation = Quaternion.Euler(entryKickerPitch, 0f, 0f);
        kicker.localScale = entryKickerWorldSize;
    }

    private static void ConfigureKickerTrigger(BoxCollider trigger)
    {
        if (trigger == null)
        {
            return;
        }

        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 2.5f, 0f);
        trigger.size = new Vector3(1f, 6f, 1f);
    }
}
