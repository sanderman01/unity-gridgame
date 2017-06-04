using System;
using UnityEngine;

namespace AmarokGames.GridGame {

    public abstract class GameSystemBase : MonoBehaviour, IGameSystem {

        public static T Create<T>() where T : MonoBehaviour {
            string name = typeof(T).Name;
            GameObject gameObject = new GameObject(name);
            T sys = gameObject.AddComponent<T>();
            return sys;
        }

        protected abstract void Disable();
        protected abstract void Enable();

        public bool Enabled { get { return this.enabled; } set { this.enabled = value; } }

        public void OnEnable() { this.Enable(); }
        public void OnDisable() { this.Disable(); }

        public abstract void TickWorld(World world, int tickRate);
        public abstract void UpdateWorld(World world, float deltaTime);
    }
}
