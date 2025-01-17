# EnhancedControls - mod for Warhammer 40000: Rogue Trader

Adds UX related stuff. 

If you're a user, downloads are on the right. See **Releases** link.
You can see detailed mod description on [Nexus page](https://www.nexusmods.com/warhammer40kroguetrader/mods/14).


## How to build

1. Get the usual tools (Visual Studio and .NET development stuff)
2. Clone the repo
3. Build. And then Build again. Project looks up your RT installation dir from your game logs (so you need to run the game at least once).

That's it. On Build output is copied into UMM folder in appdata.


## How to contribute localization

After installing the mod localization files are in `Localization` folder.   
If you named mod install folder `EnhancedControls` then full path to them would be `%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\EnhancedControls\Localization`

English localization is in `enGB.json`

Copy this file and rename new file according to the language you're translating into.
Russian  - `ruRU.json`    
German   - `deDE.json`   
French   - `frFR.json`   
Chinese  - `zhCN.json`    
Spanish  - `esES.json`   
Japanese - `jaJP.json`   

(If your system is set to hide file extensions and you don't see `.json`, then ignore that part)

Open your new localization file with any text editor - but for best results use something with syntax highlight - Notepad++, Visual Studio Code, or something else.   
Change **Text** properties to translate things.  
Start the game in chosen language to see the effect. Or switch the language in settings, the mod will reload your changes.

Send me translation file if you want me to include your translation with the mod.
