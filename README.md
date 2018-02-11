# iMessageStickerUnity

An iMessage Sticker plugin for Unity3d that adds a Sticker extension target to an Xcode project created by Unity3d.

## Unity Version
Tested with several Unity Versions
* Unity 2017.3.0f3
* Unity 2017.2.0p2
* Unity 2017.1.1f1
* Unity 5.6.4p4
* Unity 5.6.1f1
* Unity 5.6.0f3
* Unity 5.5.2f1
* Unity 5.4.5p5
* Unity 5.4.5p3
* Unity 5.4.4f1
* Unity 5.3.8f2

## Xcode Version
Tested with several Xcode Versions
* Xcode 9.3
* Xcode 9.1
* Xcode 8.3.2

## Stickers During Runtime
This plugin does not support adding stickers during runtime.

## Feedback

We would 😍 to hear your opinion about this library. Please file an issue if there's something you would like to see improved.

If you use this library and are happy with it consider sending out a tweet mentioning [@agens](https://twitter.com/agens). This library is made with love by [Skjalg S. Mæhre](https://github.com/Skjalgsm).

[<img src="http://static.agens.no/images/agens_logo_w_slogan_avenir_medium.png" width="340" />](http://agens.no/)

## First, you need to Configurate the Sticker Pack.
You do this by selecting the Sticker Pack menu item from within Unity.
This will create the StickerPack asset for you if it is not set up yet and then select it.

![alt tag](https://raw.githubusercontent.com/agens-no/iMessageStickerUnity/master/meta/Configurate.png)

## Then you can specify the settings for the sticker pack.

![alt tag](https://raw.githubusercontent.com/agens-no/iMessageStickerUnity/master/meta/StickerPackAsset.png)![alt tag](https://raw.githubusercontent.com/agens-no/iMessageStickerUnity/master/meta/CustomizingIcons.gif)

## As well as add sticker images.

![alt tag](https://raw.githubusercontent.com/agens-no/iMessageStickerUnity/master/meta/AddingStickers.png)

## Supported file types
[Apple guidelines and restrictions](https://developer.apple.com/ios/human-interface-guidelines/extensions/messaging/)
* jpg single image
* png single image *- recommended for static images*
* png sequences
* png animations (apng with .png extension) *- recommended for animations*
* gif animations

## Signing in Xcode
If you have trouble with signing issues in Xcode or are using automated builds,
then its a good idea to specify the name of the provisioning profile directly in the sticker pack.

![alt tag](https://raw.githubusercontent.com/agens-no/iMessageStickerUnity/master/meta/Signing.png)
