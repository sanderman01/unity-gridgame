﻿// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System;
using System.Collections;

namespace AmarokGames.Grids.Data {

    public class BooleanBuffer : IDataBuffer {
        private readonly BitArray buffer;
        private int lastModified;

        public BooleanBuffer(int length) {
            this.buffer = new BitArray(length);
        }

        public BufferType Type {
            get { return BufferType.Boolean; }
        }

        public int Length {
            get { return buffer.Length; }
        }

        public int LastModified {
            get { return lastModified; }
        }

        public BitArray GetRawBuffer() {
            return buffer;
        }

        public bool GetValue(int index) {
            return buffer[index];
        }

        public void SetValue(bool value, int index) {
            buffer[index] = value;
        }

        public void MarkModified(int frameCount) {
            lastModified = frameCount;
        }
    }
}
