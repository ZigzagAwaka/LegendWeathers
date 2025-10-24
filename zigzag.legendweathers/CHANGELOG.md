## 2.0.4
- **Blood Moon**
    - Added `Terrain effects volume` config, this can be used to customize the volume of the weather's terrain effects
    - Added `Ambience music volume` config, this can be used to customize the volume of the weather's ambient music
    - Added `Ambience music type` config, you can use this to replace the weather's ambient music by the vanilla Eclipsed music if wanted
- **Majora Moon**
    - f

## 2.0.3
- **General**
    - Fully removed WeatherTweaks integration as it's now managed by [Combined_Weathers_Toolkit](https://thunderstore.io/c/lethal-company/p/Zigzag/Combined_Weathers_Toolkit/)

## 2.0.2
- **Blood Moon**
    - Fixed Blood Stones not getting their correct scrap value when being spawned by a lightning bolt
- **Majora Moon**
    - Reduced the haunted pocket event timer of the Majora's Mask, so it will try to get in your active inventory space more often
    - Reduced the random audio timer of the Majora's Mask when it's equiped by a Masked enemy, so it will laugh more often (this does not apply when the mask is not yet equiped, the timer in this case is unchanged)

## 2.0.1
- **General**
    - Updated networking to work for v73 of Lethal Company

## 2.0.0 Blood Moon release
- **General**
    - Added Blood Moon weather
    - Added 2 general configs to customize weather warning messages and WeatherTweaks integration
- **Majora Moon**
    - Reduced light intensity and audio volume of Moon's Tears
    - Improved the Majora Moon position in the sky for the following moons : Offense, Berunah, Repress, Roart, Faith, Core and Dreck
    - Majora Moon's solid collision will now be disabled when the ship is leaving before the final explosion
    - Improved the Majora Moon falling formula, this will improve visuals for the falling effect during specific situations
    - Updated the Majora Moon falling effect to now be compatible with slower time multipliers
    - Updated the Majora Moon falling effect to now be compatible with in-game moon's clock modifications (time shifts and similar other effects)
    - Added compatibility for the Majora Moon falling effect when modifying the moon's actual time using [Imperium](https://thunderstore.io/c/lethal-company/p/giosuel/Imperium/)
    - Masked enemies invocation when using the Majora's Mask will now all be the same player (the one who used the item)
    - Fixed Majora sky sometimes not being correctly enabled when the weather is spawned
    - The Majora crash sequence music will now not play if the config's volume is equal to 0
    - Optimized code for the weather warning messages system
    - Optimized code related to the rendering of sky effects
- **Found issues**
    - Something unexpected will happen when playing `Oath to Order` with the Ocarina item, please avoid playing this song until a proper fix is made by [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry/)

<details><summary>Previous changes (click to reveal)</summary>

## 1.1.11
- Added custom colors for some combined weathers names
- Updated `LegendWeather.WeatherInfo` to now be replaced with `WeatherDefinition` provided by [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry/) 0.7.0+
- All patches related to the Majora Moon will now do nothing if the Majora Moon weather is disabled in the config

## 1.1.10
- Hotfix 2 for Company moons

## 1.1.9
- Updated combined weathers integration

## 1.1.8
- Hotfix for Company moons

## 1.1.7
- When the Faceless model version is activated in the config, the Majora's Mask will now be reskined to match the vanilla look of the moon
- Fixed `Automatic model selection` config being broken since last update
- Added new combined weathers effects when [CodeRebirth](https://thunderstore.io/c/lethal-company/p/XuXiaolan/CodeRebirth/) or [MrovWeathers](https://thunderstore.io/c/lethal-company/p/mrov/MrovWeathers/) are installed
- Improved the Majora Moon position in the sky for the following moons : Triskelion, March and Adamance
- The Majora Timer UI is now more accurate by 1 second
- The weather will now work on Company moons when activating `Company Moon compatibility` config (false by default). You may also need to remove the company moons names in the blacklist section of WeatherRegistery

## 1.1.6
- Added [WeatherTweaks](https://thunderstore.io/c/lethal-company/p/mrov/WeatherTweaks/) integration : 4 combined weathers effects, and 5 more if you have [LethalElementsBeta](https://thunderstore.io/c/lethal-company/p/v0xx/LethalElementsBeta/) installed
- Added Joy, Dice and Baldy model version (can be activated in the config)
- Improved the Majora Moon position in the sky for the following moons : EGypt, Noctis, Terra and Luigis Mansion
- Fixed the eclipsed sun texture not being detected during a combined weather effect between Eclipsed and Majora Moon
- Adjusted the Majora Timer UI position a bit

## 1.1.5
- Recompiled for v70

## 1.1.4
- Fixed `Automatic model selection` config with new conditions so it should now be synced between players

## 1.1.3
- Added Boomy, Owl and Abibabou model version (can be activated in the config)
- Added `Automatic model selection` config, you can activate it to have the model be automatically selected based on your installed mods and some specific conditions
- Re-added the Majora weather warning message, now fixed with a custom system that will not cause message display issues

## 1.1.2
- Improved the Majora Moon position in the sky for Skelaah's Wild Moons
- The weather is now blacklisted on Black Mesa
- Removed the Majora weather warning message (that is displayed when you land on a planet for the first time with the Majora Moon active). Will be re-added if the vanilla 'save display tip code' is fixed in the future

## 1.1.1
- Removed dead players end of round camera effects

## 1.1.0
- Added compatibility for the Ocarina item in [ChillaxScraps](https://thunderstore.io/c/lethal-company/p/Zigzag/ChillaxScraps/)
    - Play `Oath to Order` in altitude during the final timer to play a special animation !
    - Sun's Song is now banned when the Majora Moon is active
- Planet fog removal during the Majora weather is now handled by the Majora sky effect directly instead of the Moon
    - This change allows the planet fog to be reverted during specific events, such as when going into the facility
    - Inside fog should no longer be removed
- Improved the Majora Timer UI position for different screen resolution
- Added a bit of bloom in the Majora purple sky
- The moon surface material is no longer double-sided
- Adjusted Moon's Tears audio a bit

## 1.0.2
- Added the Faceless model version (can be activated in the config)
- Improved the Majora Moon position in the sky for the following : Moons of Otherworldly Oddity, Nightmare Moons, Harvest Moons, Kast, Ganimedes, Sector-0, Secret Labs, Orion, Aquatis, Wither

## 1.0.1
- Fixed texture on the Majora Head for the N64 model version
- Fixed Masked enemies invocation having the wrong player suit when playing in singleplayer
- The weather is now blacklisted on Cosmocos
- Improved the Majora Moon default position in the sky for custom moons
- Improved wind speed (sky clouds speed) modification on custom moons
- Improved the Majora Moon position in the sky for almost all Wesley's Moons

## 1.0.0 Initial release
- Added Majora Moon weather

</details>