using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchdruidsAdditions.Data;

public static class SaveStateData
{
    public static Dictionary<SlugcatStats.Name, SaveStateDataContainer> saveStateData = [];
    public class SaveStateDataContainer
    {
        public SlugcatStats.Name name;
        public SaveState parasiteSaveState;
        public bool infected;

        public SaveStateDataContainer(SlugcatStats.Name name)
        {
            this.name = name;
            saveStateData.Add(name, this);
        }
    }
}
