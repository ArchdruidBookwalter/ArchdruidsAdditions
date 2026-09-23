namespace ArchdruidsAdditions.Hooks;

public static class IteratorHooks
{
    internal static void On_MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        orig(self);
        if (self.id == Conversation.ID.Moon_Misc_Item)
        {
            var item = self.describeItem;
            if (item == MiscItemType.ScarletFlowerBulb)
            {
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "This is the spore-releasing bulb of a mycelial root network." +
                    "<LINE>My creators used to call it Monksweed, as it frustrated them to no end whenever it managed to grow inside their old temples and monasteries." +
                    "<LINE>The roots can extend for miles below the surface, and are nearly impossible to remove once they infect an area."

                    ), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "Some believed the spores could make you hear strange whispers, or give you visions of the past and future if you inhaled them." +
                    "<LINE>I don't know how true those rumors were, but I would be extremely careful with this if I were you."

                    ), 0));
            }
            else if (item == MiscItemType.ParrySword)
            {
                self.events.Add(new Conversation.TextEvent(self, 80, self.Translate(

                    "Oh, what a curious find, <PLAYERNAME>... This weapon was once a personal item of one of my long-departed creators." +
                    "<LINE>Its purpose was for self-defence, as its curved design and superior durability enables it to absorb and deflect blows from stronger opponents."

                    ), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "You see, my creators believed that violence was the most base and unforgivable of all vices. Conflict was rare but not unheard of," +
                    "<LINE>so weapons like these were made to protect important persons from harm without comitting such harm in return."

                    ), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "This one was even equipped with a primitive AI module, to prevent it from being used for evil, I assume." +
                    "<LINE>It must trust you, if you were able to touch it and bring it all the way here. Perhaps it even considers you to be its new master."

                    ), 0));
            }
            else if (item == MiscItemType.Potato)
            {
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "This is just a plant... What else is there to say about it? I suppose the roots look edible enough for you to eat."

                    ), 0));
            }
            else if (item == MiscItemType.Bow)
            {
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "It's a bow, made from scavenged materials. The design is primitive," +
                    "<LINE>but I assume it must be effective enough to be useful to you, <PLAYERNAME>."

                    ), 0));
            }
            else if (item == MiscItemType.AshPepper)
            {
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "It looks like some kind of hot pepper, no doubt loaded with capsaicin." +
                    "<LINE>...I would be careful eating this if I were you."

                    ), 0));
            }
            else if (item == MiscItemType.LightningFruit)
            {
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "What sort of fruit is this? It seems to contain some kind of electrical charge..." +
                    "<LINE>Perhaps it is an odd defence mechanism against predatory insects."

                    ), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "My creators left behind so many strange things when they departed. Plants, animals, machines... Sometimes a mix of all three." +
                    "<LINE>It's hard to imagine all the different ways they could have evolved since then."

                    ), 0));
            }
        }
    }
    internal static SLOracleBehaviorHasMark.MiscItemType On_SLOracleBehaviorHasMark_TypeOfMiscItem(On.SLOracleBehaviorHasMark.orig_TypeOfMiscItem orig, SLOracleBehaviorHasMark self, PhysicalObject obj)
    {
        if (obj is ScarletFlowerBulb)
        { return MiscItemType.ScarletFlowerBulb; }

        if (obj is ParrySword)
        { return MiscItemType.ParrySword; }

        if (obj is Potato)
        { return MiscItemType.Potato; }

        if (obj is Bow)
        { return MiscItemType.Bow; }

        if (obj is AshPepper)
        { return MiscItemType.AshPepper; }

        if (obj is LightningFruit)
        { return MiscItemType.LightningFruit; }

        return orig(self, obj);
    }
    internal static void On_SLOracleBehavior_Update(On.SLOracleBehavior.orig_Update orig, SLOracleBehavior self, bool eu)
    {
        orig(self, eu);
        if (self.holdingObject != null)
        {
            if (self.holdingObject is ParrySword sword)
            {
                sword.rotation = new(1f, 0f);
                sword.spinning = false;
            }
            else if (self.holdingObject is Bow bow)
            {
                bow.rotation = Vector2.Lerp(bow.rotation, Custom.DirVec(self.oracle.bodyChunks[0].pos, bow.firstChunk.pos), 0.5f);
            }
        }
    }
}
