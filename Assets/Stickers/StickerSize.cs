using System;

namespace Agens.Stickers
{
    /// <summary>
    /// Messages supports three sticker sizes, which are displayed in a grid-based browser.
    /// </summary>
    [Serializable]
    public enum StickerSize
    {
        /// <summary>
        /// 100 x 100 points @3x (300 x 300 pixels).
        /// </summary>
        Small,
        /// <summary>
        /// 136 x 136 points @3x (408 x 408 pixels).
        /// </summary>
        Medium,
        /// <summary>
        /// 206 x 206 points @3x (618 x 618 pixels).
        /// </summary>
        Large
    }
}