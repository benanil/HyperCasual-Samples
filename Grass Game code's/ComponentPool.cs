
using System;
using UnityEngine;

namespace AnilTools
{
    // an object pool system for components
    public class ComponentPool<C> where C : Component
    {
        private readonly C[] components;
        private int index;

        // Pops a component in pool
        public C Get()
        {
            return components[++index % components.Length];
        }

        public ComponentPool(in int size, Transform parent, Action<C> OnSpawn = null)
        {
            this.components = new C[size];
            for (; index < size; index++)
            {
                components[index] = new GameObject(typeof(C).Name).AddComponent<C>();
                components[index].transform.parent = parent;
                OnSpawn?.Invoke(components[index]);
            }
            index = 0;
        }

        public ComponentPool(in int size, Action<C> OnSpawn = null) : this(size, new GameObject($"{typeof(C).Name} Pool").transform, OnSpawn) { }

        public ComponentPool(in int size, C prefab, Action<C> onSpawn = null)
        {
            this.components = new C[size];
            var parent = new GameObject($"{prefab.name} Pool").transform;
            for (; index < components.Length; index++)
            {
                components[index] = UnityEngine.Object.Instantiate(prefab, parent);
                onSpawn?.Invoke(components[index]);
            }
            index = 0;
        }
    }

    // an object pool system for components
    public class TransformPool
    {
        private readonly Transform[] Transforms;
        private int index;

        // Pops a component in pool
        public Transform Get()
        {
            return Transforms[++index % Transforms.Length];
        }

        public TransformPool(in int size, Transform parent, Action<Transform> OnSpawn = null, string name = "pooled transform")
        {
            this.Transforms = new Transform[size];
            for (; index < size; index++)
            {
                Transforms[index].parent = parent;
                OnSpawn?.Invoke(Transforms[index]);
            }
            index = 0;
        }

        public TransformPool(in int size, Action<Transform> OnSpawn = null) : this(size, new GameObject("Transform Pool").transform, OnSpawn) { }

        public TransformPool(in int size, Transform prefab, Action<Transform> onSpawn = null)
        {
            this.Transforms = new Transform[size];
            var parent = new GameObject($"{prefab.name} Pool").transform;
            for (; index < Transforms.Length; index++)
            {
                Transforms[index] = UnityEngine.Object.Instantiate(prefab, parent);
                onSpawn?.Invoke(Transforms[index]);
            }
            index = 0;
        }
    }
}