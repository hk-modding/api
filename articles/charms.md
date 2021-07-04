Charms
======

Ownership of charms can be checked with the `PlayerData.instance.gotCharm_X`
var, where X is the ID found in the following list. Checking if they are
equipped can be checked with `PlayerData.instance.equippedCharm_X`. The cost to
equip charms can be changed with `PlayerData.instance.charmCost_X`.

> [!Note]
> Using `PlayerData.instance.GetBool("gotCharm_X")` or
> `PlayerData.instance.GetInt("charmCost_X")` is prefered, as that way 
> other mods can make their changes to the desired values.

Overriding values
=================
You can directly set `PlayerData` values, however, this permanently changes a
save file. It's often better to use the [`GetIntHook`](xref:Modding.ModHooks.GetIntHook).
This makes the change only apply when the mod is active, which is generally preferable.

```cs
using Modding;

// This changes the cost of Wayward Compass to 0
public class Example : Mod {
    public override void Initialize() {
        ModHooks.GetIntHook += GetInt;
    }

    private int GetInt(string name, int orig) {
        return name == nameof(PlayerData.charmCost_2)
            ? 0
            : orig;
    }
}
```

For the actual behaviour of charms, it varies from charm-to-charm. However,
a large amount can be found on HeroController constants such as `RUN_SPEED_CH`
and `RUN_SPEED_CH_COMBO` for Sprintmaster, `GRUB_SOUL_MP` and `GRUB_SOUL_MP`
for Grubsong, `ATTACK_DURATION_CH` for Quickslash, and more.

Sprites
=======
If you mean to replace a charm, then you can change the sprites used for it.
There are two main areas a charm sprite pops up, one being the inventory and
the other being shops. You can change them as follows:

# [Inventory](#tab/tabid-1)
Given a sprite, one can replace it in the inventory using `CharmIconList`,
which allows you to just assign to its sprite list, indexed by charm ID.
```cs
CharmIconList.Instance.spriteList[i] = sprite;
```
A few charms, such as grimmchild and the unbreakable charms have separate
fields in the CharmIconList class.

# [Shops](#tab/tabid-2)
Shops are a little more complicated, but can be most easily done using
the `On` hook for `ShopItems.Awake`. For brevity, only the handler will
be shown below.
```
// An example of storing multiple sprites which your mod would fill.
private Dictionary<int, Sprite> _sprites = new();

private void Awake(On.ShopItemStats.orig_Awake orig, ShopItemStats self) {
    orig(self);

    string pd_bool = self.playerDataBoolname;

    if (!pd_bool.StartsWith("gotCharm_"))
        return;

    if (!_sprites.TryGetValue(key, out var sprite))
        return;

    var go = ReflectionHelper.GetField<ShopItemStats, GameObject>(self, "itemSprite");
    
    go.GetComponent<SpriteRenderer>().sprite = sprite;
}
```
***

Values
======

<table class="docutils" border="1">
    <colgroup>
        <col width="5%">
        <col width="36%">
        <col width="23%">
        <col width="37%">
    </colgroup>
    <thead valign="bottom">
        <tr class="row-odd">
            <th class="head">ID</th>
            <th class="head">Name</th>
            <th class="head">LangName</th>
            <th class="head">Add.Bool</th>
        </tr>
    </thead>
    <tbody valign="top">
        <tr class="row-even">
            <td>01</td>
            <td>Gathering Swarm</td>
            <td>CHARM_NAME_1</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>02</td>
            <td>Wayward Compass</td>
            <td>CHARM_NAME_2</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>03</td>
            <td>Grubsong</td>
            <td>CHARM_NAME_3</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>04</td>
            <td>Stalwart Shell</td>
            <td>CHARM_NAME_4</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>05</td>
            <td>Baldur Shell</td>
            <td>CHARM_NAME_5</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>06</td>
            <td>Fury of the Fallen</td>
            <td>CHARM_NAME_6</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>07</td>
            <td>Quick Focus</td>
            <td>CHARM_NAME_7</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>08</td>
            <td>Lifeblood Heart</td>
            <td>CHARM_NAME_8</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>09</td>
            <td>Lifeblood Core</td>
            <td>CHARM_NAME_9</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>10</td>
            <td>Defender’s Crest</td>
            <td>CHARM_NAME_10</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>11</td>
            <td>Flukenest</td>
            <td>CHARM_NAME_11</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>12</td>
            <td>Thorns of Agony</td>
            <td>CHARM_NAME_12</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>13</td>
            <td>Mark of Pride</td>
            <td>CHARM_NAME_13</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>14</td>
            <td>Steady Body</td>
            <td>CHARM_NAME_14</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>15</td>
            <td>Heavy Blow</td>
            <td>CHARM_NAME_15</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>16</td>
            <td>Sharp Shadow</td>
            <td>CHARM_NAME_16</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>17</td>
            <td>Spore Shroom</td>
            <td>CHARM_NAME_17</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>18</td>
            <td>Longnail</td>
            <td>CHARM_NAME_18</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>19</td>
            <td>Shaman Stone</td>
            <td>CHARM_NAME_19</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>20</td>
            <td>Soul Catcher</td>
            <td>CHARM_NAME_20</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>21</td>
            <td>Soul Eater</td>
            <td>CHARM_NAME_21</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>22</td>
            <td>Glowing Womb</td>
            <td>CHARM_NAME_22</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td rowspan="3">23</td>
            <td>Fragile Heart</td>
            <td>CHARM_NAME_23</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>Fragile Heart (Repair)</td>
            <td>CHARM_NAME_23_BRK</td>
            <td>brokenCharm_23</td>
        </tr>
        <tr class="row-even">
            <td>Unbreakable Heart</td>
            <td>CHARM_NAME_23_G</td>
            <td>fragileHeart_unbreakable</td>
        </tr>
        <tr class="row-odd">
            <td rowspan="3">24</td>
            <td>Fragile Greed</td>
            <td>CHARM_NAME_24</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>Fragile Greed (Repair)</td>
            <td>CHARM_NAME_24_BRK</td>
            <td>brokenCharm_24</td>
        </tr>
        <tr class="row-odd">
            <td>Unbreakable Greed</td>
            <td>CHARM_NAME_24_G</td>
            <td>fragileGreed_unbreakable</td>
        </tr>
        <tr class="row-even">
            <td rowspan="3">25</td>
            <td>Fragile Strength</td>
            <td>CHARM_NAME_25</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>Fragile Strength (Repair)</td>
            <td>CHARM_NAME_25_BRK</td>
            <td>brokenCharm_25</td>
        </tr>
        <tr class="row-even">
            <td>Unbreakable Strength</td>
            <td>CHARM_NAME_25_G</td>
            <td>fragileStrength_unbreakable</td>
        </tr>
        <tr class="row-odd">
            <td>26</td>
            <td>Nailmaster’s Glory</td>
            <td>CHARM_NAME_26</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>27</td>
            <td>Joni’s Blessing</td>
            <td>CHARM_NAME_27</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>28</td>
            <td>Shape of Unn</td>
            <td>CHARM_NAME_28</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>29</td>
            <td>Hiveblood</td>
            <td>CHARM_NAME_29</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>30</td>
            <td>Dream Wielder</td>
            <td>CHARM_NAME_30</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>31</td>
            <td>Dashmaster</td>
            <td>CHARM_NAME_31</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>32</td>
            <td>Quick Slash</td>
            <td>CHARM_NAME_32</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>33</td>
            <td>Spell Twister</td>
            <td>CHARM_NAME_33</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>34</td>
            <td>Deep Focus</td>
            <td>CHARM_NAME_34</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>35</td>
            <td>Grubberfly’s Elegy</td>
            <td>CHARM_NAME_35</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td rowspan="3">36</td>
            <td>White Fragment</td>
            <td>CHARM_NAME_36_A</td>
            <td rowspan="2">royalCharmState</td>
        </tr>
        <tr class="row-even">
            <td>Kingsoul</td>
            <td>CHARM_NAME_36_B</td>
        </tr>
        <tr class="row-odd">
            <td>Void Heart</td>
            <td>CHARM_NAME_36_C</td>
            <td>gotShadeCharm</td>
        </tr>
        <tr class="row-even">
            <td>37</td>
            <td>Sprintmaster</td>
            <td>CHARM_NAME_37</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td>38</td>
            <td>Dreamshield</td>
            <td>CHARM_NAME_38</td>
            <td>None</td>
        </tr>
        <tr class="row-even">
            <td>39</td>
            <td>Weaversong</td>
            <td>CHARM_NAME_39</td>
            <td>None</td>
        </tr>
        <tr class="row-odd">
            <td rowspan="2">40</td>
            <td>Grimmchild</td>
            <td>CHARM_NAME_40</td>
            <td rowspan="2">grimmChildLevel</td>
        </tr>
        <tr class="row-even">
            <td>Carefree Melody</td>
            <td>CHARM_NAME_40_N</td>
        </tr>
    </tbody>
</table>
