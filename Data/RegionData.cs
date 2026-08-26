using System.Collections.Generic;
using System.IO;

namespace ArchdruidsAdditions.Data
{
    public class RegionData
    {
        public static Dictionary<string, string[]> regionDataList = [];

        public RegionData(RainWorldGame game, SlugcatStats.Timeline time)
        {
            regionDataList.Clear();

            Region[] regions = Region.LoadAllRegions(time, game);
            foreach (Region region in regions)
            {

                string fileLocation = AssetManager.ResolveFilePath(string.Concat(
                [
                    "World",
                    Path.DirectorySeparatorChar.ToString(),
                    region.name,
                    Path.DirectorySeparatorChar.ToString(),
                    "properties" + "-" + time.value + ".txt",
                ]));

                if (!File.Exists(fileLocation))
                {
                    fileLocation = AssetManager.ResolveFilePath(string.Concat(
                    [
                        "World",
                        Path.DirectorySeparatorChar.ToString(),
                        region.name,
                        Path.DirectorySeparatorChar.ToString(),
                        "properties.txt",
                    ]));
                }

                string regionData = File.ReadAllText(fileLocation);
                string[] splitRegionData = regionData.Split('\n');
                regionDataList.Add(region.name, splitRegionData);
            }
        }

        public string ReadRegionData(string regionName, string variableName)
        {
            //Debug.Log("<Archduid's Additions> SEARCHING FOR REGION DATA: \'" + regionName + "\' - \'" + variableName + "\'");

            string[] data = regionDataList[regionName];
            foreach (string line in data)
            {
                //Debug.Log("<Archduid's Additions>    " + line);

                if (line.StartsWith(variableName))
                {
                    //Debug.Log("<Archduid's Additions> FOUND DATA!");

                    return line.Remove(0, variableName.Length + 2);
                }
            }

            //Debug.Log("<Archduid's Additions> FAILED TO FIND DATA. RETURNING NULL");

            return null;
        }
    }
}
