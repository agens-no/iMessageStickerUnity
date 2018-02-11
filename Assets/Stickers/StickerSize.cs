using System;

namespace Agens.Stickers
{
    /// <summary>
    /// Messages supports three sticker sizes, which are displayed in a grid-based browser.
    /// More information: https://developer.apple.com/ios/human-interface-guidelines/extensions/messaging/
    /// </summary>
    [Serializable]
    public enum StickerSize
    {
        /// <summary>
        /// 100 x 100 points @3x (300 x 300 pixels).
        /// In the browser 4 stickers will be in one row.
        /// </summary>
        Small,
        /// <summary>
        /// 136 x 136 points @3x (408 x 408 pixels).
        /// In the browser 3 stickers will be in one row.
        /// </summary>
        Medium,
        /// <summary>
        /// 206 x 206 points @3x (618 x 618 pixels).
        /// In the browser 2 stickers will be in one row.
        /// </summary>
        Large
    }
}