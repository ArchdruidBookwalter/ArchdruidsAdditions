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
                    "<LINE>My creators used to call it Monksweed, as it frustrated them to no end whenever it managed to grow inside their old temples and monasteries." +
                    "<LINE>The roots can extend for miles below the surface, and are nearly impossible to remove once they infect an area."

                    ), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "Some believed the spores could make you hear strange whispers, or give you visions of the past and future if you inhaled them." +
                    "<LINE>I don't know how true those rumors were, but I would be extremely careful with this if I were you."

                    ), 0));
            }
            if (item == Enums.MiscItemType.ParrySword)
            {
                self.events.Add(new Conversation.TextEvent(self, 80, self.Translate(

                    "Is... Is this the blade of Five Sacred Sigils upon a Mountain!?" +
                    "<LINE>In times long past, this was one of the most holy and revered relics ever held by my creators! How ever did you manage to find it!?"

                    ), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "They once said that it could only be wielded by one who had achieved the highest level of enlightenment." +
                    "<LINE>I do not know how you would even be able to touch it, much less carry it all the way here, unless..."

                    ), 0));
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(

                    "...Perhaps there is more to you than what meets the eye, <PLAYERNAME>."

                    ), 0));
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
