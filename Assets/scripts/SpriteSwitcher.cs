using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpriteSwitcher : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public Sprite sprite;
        public string name;
        public AudioClip sound;
    }

    [SerializeField] private SpriteRenderer target;
    [SerializeField] private Vector2 fitSize = Vector2.one;
    [SerializeField] private Entry[] entries = new Entry[0];
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private AudioSource source;
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;
    [SerializeField] private int index;

    public int Index => index;

    private void Awake()
    {
        target ??= GetComponent<SpriteRenderer>();

        if (!target)
        {
            enabled = false;
            return;
        }

        Apply(false);
    }

    private void Update()
    {
        foreach (Keyboard keyboard in InputSystem.devices)
        {
            for (int i = 0; i < 9; i++)
            {
                if (keyboard[Key.Digit1 + i].wasPressedThisFrame)
                {
                    SetIndex(i);
                }
            }
        }
    }

    public void SetIndex(int value)
    {
        index = value;
        Apply(true);
    }

    private void Fit(Sprite sprite)
    {
        if (!sprite)
            return;

        Vector2 size = sprite.bounds.size;

        if (size.x <= 0f || size.y <= 0f)
            return;

        Vector3 parentScale = transform.parent ? transform.parent.lossyScale : Vector3.one;

        float px = Mathf.Approximately(parentScale.x, 0f) ? 1f : Mathf.Abs(parentScale.x);
        float py = Mathf.Approximately(parentScale.y, 0f) ? 1f : Mathf.Abs(parentScale.y);

        float scale = Mathf.Min(fitSize.x / (size.x * px), fitSize.y / (size.y * py));

        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void Apply(bool playSound)
    {
        if (entries.Length == 0)
            return;

        index = Mathf.Clamp(index, 0, entries.Length - 1);

        Entry entry = entries[index];

        target.sprite = entry.sprite;

        Fit(entry.sprite);

        if (nameLabel)
            nameLabel.text = entry.name;

        if (!playSound || !entry.sound)
            return;

        if (source)
            source.PlayOneShot(entry.sound, volume);
        else
            AudioSource.PlayClipAtPoint(entry.sound, transform.position, volume);
    }
}
