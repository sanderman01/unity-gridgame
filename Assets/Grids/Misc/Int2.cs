// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System;
using UnityEngine;

namespace AmarokGames {

    [Serializable]
    public struct Int2 : IEquatable<Int2> {

        public int x;
        public int y;

        public Int2(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Int2(float x, float y) {
            this.x = (int)x;
            this.y = (int)y;
        }

        public bool Equals(Int2 other) {
            return this.x == other.x && this.y == other.y;
        }

        public override bool Equals(object obj) {
            if (obj is Int2) {
                Int2 other = (Int2)obj;
                return this.Equals(other);
            } else return false;
        }

        public override int GetHashCode() {
            return 31 * this.x + this.y;
        }

        public override string ToString() {
            return string.Format("({0}, {1})", this.x, this.y);
        }

        public static Int2 operator +(Int2 a, Int2 b) {
            return new Int2(a.x + b.x, a.y + b.y);
        }

        public static Int2 operator -(Int2 a, Int2 b) {
            return new Int2(a.x - b.x, a.y - b.y);
        }

        public static implicit operator Vector2(Int2 a) {
            return new Vector2(a.x, a.y);
        }

        public static implicit operator Vector3(Int2 a) {
            return new Vector3(a.x, a.y, 0);
        }

        public static bool operator ==(Int2 a, Int2 b) { return a.x == b.x && a.y == b.y; }
        public static bool operator !=(Int2 a, Int2 b) { return a.x != b.x || a.y != b.y; }
    }
}
