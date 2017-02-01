using System;
using System.Collections.Generic;
using UnityEngine;

namespace Agens.Stickers
{
    public class Sticker : ScriptableObject
    {
        public string Name;
        public int Fps = 15;
        public int Repetitions = 0;
        public int Index;
        public bool Sequence;
        public List<Texture2D> Frames;

        public void CopyFrom(Sticker sticker, int i)
        {
            name = sticker.Name;
            Name = sticker.Name;
            Fps = sticker.Fps;
            Repetitions = sticker.Repetitions;
            Index = i;
            Sequence = sticker.Sequence;
            Frames = sticker.Frames;
        }
    }
}