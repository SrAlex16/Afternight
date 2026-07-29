using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cualquier objeto que se gestione desde un ObjectPool debe implementar esto.
/// OnSpawned se llama al activarlo (reemplaza el "SetDirection"/"ActiveProjectile" a medida
/// que tenía cada proyectil). OnDespawned se llama al devolverlo al pool.
/// </summary>
public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}

/// <summary>
/// Pool de objetos genérico. Sustituye el patrón repetido de "array fijo de GameObjects +
/// FindFireball() recorriendo linealmente buscando uno inactivo" que había en PlayerAttack,
/// ScreenAttackController y ArrowTrap.
///
/// Uso típico:
///   [SerializeField] private Fireball fireballPrefab;
///   private ObjectPool&lt;Fireball&gt; pool;
///   void Awake() => pool = new ObjectPool&lt;Fireball&gt;(fireballPrefab, 8, transform);
///   ...
///   Fireball fb = pool.Get();
///   fb.transform.position = firePoint.position;
///   fb.Launch(direction);   // el propio objeto llama a pool.Release(this) cuando termina
/// </summary>
public class ObjectPool<T> where T : Component, IPoolable
{
    private readonly T prefab;
    private readonly Queue<T> available = new Queue<T>();
    private readonly Transform parent;
    private readonly List<T> all = new List<T>(); // por si hace falta hacer crecer el pool

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        if (prefab == null)
        {
            GameLogger.Error(GameLogger.Category.Pool, $"ObjectPool<{typeof(T).Name}> creado sin prefab asignado.");
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            CreateNew();
        }

        GameLogger.Info(GameLogger.Category.Pool, $"Pool de {typeof(T).Name} inicializado con {initialSize} instancias.");
    }

    private T CreateNew()
    {
        T instance = Object.Instantiate(prefab, parent);
        instance.gameObject.SetActive(false);
        available.Enqueue(instance);
        all.Add(instance);
        return instance;
    }

    public T Get()
    {
        if (prefab == null)
        {
            GameLogger.Error(GameLogger.Category.Pool, $"No se puede hacer Get() en ObjectPool<{typeof(T).Name}>: prefab nulo.");
            return null;
        }

        T instance = available.Count > 0 ? available.Dequeue() : CreateNew();

        if (available.Count == 0)
        {
            GameLogger.Verbose(GameLogger.Category.Pool,
                $"Pool de {typeof(T).Name} sin instancias libres, se ha creado una nueva (total: {all.Count}). " +
                "Considera aumentar el tamaño inicial si esto pasa a menudo.");
        }

        instance.gameObject.SetActive(true);
        instance.OnSpawned();
        return instance;
    }

    public void Release(T instance)
    {
        if (instance == null) return;

        instance.OnDespawned();
        instance.gameObject.SetActive(false);
        available.Enqueue(instance);
    }
}
