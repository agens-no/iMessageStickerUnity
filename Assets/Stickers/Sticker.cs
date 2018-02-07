using System;
using System.Collections.Generic;
using UnityEngine;

namespace Agens.Stickers
{
    public class Sticker : ScriptableObject
    {
        [Tooltip("Name of the sticker")]
        public string Name;
        [Tooltip("Frames per second. Apple recommends 15+ FPS")]
        public int Fps = 15;
        [Tooltip("Number of repetitions (0 being infinite cycles")]
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