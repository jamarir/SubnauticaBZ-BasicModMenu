# SubnauticaBZ-BasicModMenu
Basic Subnautica Below Zero Mod Menu PoC with dnSpy

This basic mod menu is used as a reference for [this blog post article](https://jamarir.hashnode.dev/gamehacking-subnautica-below-zero-unity3d-modding-dnspy-tips-and-tricks), which contains 3 cheats:
- Infinite Oxygen: Increments the player's oxygen every frame.
- Infinite Temperature: Increments the player's temperature every frame.
- Movement Speed: Disable the player's movement speed limit every frame.

To use that mod menu, a class must be created in the Subnautica Below Zero's `Assembly-CSharp.dll` with dnSpy, in which the `ModMenu.cs` content will be copied. 
> As a side note, the `dumpDir` attribute has been set to `D:/Modding/`. That folder should be updated to any existant folder, if `D:/Modding/` doesn't exist.

Then, a globally used `OnGUI()` method can be hooked, e.g. `WaterBiomeManager.OnGUI()`, which calls the `ModMenuManager()` function as follows:
```cs
ModMenu.ModMenuManager();
```

Finally, while in-game, the `F10` hotkey can be pressed to lock / unlock the cursor and interact with the mod menu.