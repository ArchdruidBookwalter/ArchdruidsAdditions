using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchdruidsAdditions.Hooks;

public static class IteratorHooks
{
    internal static void On_MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        orig(self);
        if (self.id == Conversation.ID.Moon_Misc_Item)
        {
            var item = self.describeItem;
            if (item == Enums.MiscItemType.ScarletFlowerBulb)
            {
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "This is the spore-releasing bulb of a mycelial root network." +
                    "<LINE>My creators used to call it Monksweed, as it frustrated them to no end whenever it managed to grow inside their old temples and monasteries."

                    ), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "Some believed the spores could make you hear strange whispers, or give you visions of the past and future if you inhaled them." +
                    "<LINE>I don't know how true those rumors were, but I would be extremely careful with this if I were you."

                    ), 0));
            }
            if (item == Enums.MiscItemType.ParrySword)
            {
                self.events.Add(new Conversation.TextEvent(self, 80, self.Translate(

                    "Is... Is this the blade of Five Sacred Sigils upon a Mountain?!?" +
                    "<LINE>How in all the six realms did you even manage to find it, much less bypass its defenses!?"

                    ), 0));
                if (self.myBehavior.player.KarmaCap == 9)
                {
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "...My creators once said that it could only be wielded by one who had achieved the highest level of enlightenment." +
                    "<LINE>You do not seem particularly grand or noble, but now that I think about it, your presence does bring an odd sense of tranquility."

                    ), 0));
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "Perhaps I misjudged you, <PLAYERNAME>."

                    ), 0));
                }
                else
                {
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "<CAPPLAYERNAME>, I am not normally one for superstition, but you must return this immediately." +
                    "<LINE>This was one of the most sacred relics of my creators. If anything could summon their wrath from the depths of the Void, it would be this."

                    ), 0));
                }
            }
        }
    }
    internal static SLOracleBehaviorHasMark.MiscItemType On_SLOracleBehaviorHasMark_TypeOfMiscItem(On.SLOracleBehaviorHasMark.orig_TypeOfMiscItem orig, SLOracleBehaviorHasMark self, PhysicalObject obj)
    {
        if (obj is Objects.ScarletFlowerBulb)
        {
            return Enums.MiscItemType.ScarletFlowerBulb;
        }
        if (obj is Objects.ParrySword)
        {
            return Enums.MiscItemType.ParrySword;
        }
        return orig(self, obj);
    }
}
