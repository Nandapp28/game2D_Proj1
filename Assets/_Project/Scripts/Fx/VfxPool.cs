// P7 — memakai ulang objek efek alih-alih Instantiate/Destroy setiap serangan (UnityEngine.Pool).
using UnityEngine;
using UnityEngine.Pool;

public class VfxPool : MonoBehaviour
{
    [SerializeField] private SpriteVfx prefab;

    private ObjectPool<SpriteVfx> pool;

    public int CountActive => pool.CountActive;
    public int CountAll => pool.CountAll;

    void Awake()
    {
        pool = new ObjectPool<SpriteVfx>(
            createFunc: () => Instantiate(prefab, transform),
            actionOnGet: v => v.gameObject.SetActive(true),
            actionOnRelease: v => v.gameObject.SetActive(false),
            actionOnDestroy: v => Destroy(v.gameObject),
            collectionCheck: true,
            defaultCapacity: 4,
            maxSize: 16);
    }

    public void Play(Vector3 position)
    {
        SpriteVfx vfx = pool.Get();
        vfx.transform.position = position;
        vfx.Play(() => pool.Release(vfx));
    }
}
