using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArchdruidsAdditions.RegionData
{
    public class RegionData
    {
        public List<string[]> regionDataList;

        public RegionData(RainWorldGame game, SlugcatStats.Timeline time)
        {
            if (!game.IsArenaSession)
            {
                regionDataList = LoadAllRegionData(time);
            }
        }

        public List<string[]> LoadAllRegionData(SlugcatStats.Timeline time)
        {
            List<string[]> regionDataList = [];

            Region[] regions = Region.LoadAllRegions(time);
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

                string[] newRegion = [region.name, File.ReadAllText(fileLocation)];
                regionDataList.Add(newRegion);

            }
            return regionDataList;
        }

        public string GetSpecificRegionData(string regionName)
        {
            foreach (string[] regionData in regionDataList)
            {
                if (regionData[0] == regionName)
                {
                    return regionData[1];
                }
            }
            return "";
        }

        public string ReadRegionData(string regionName, string variableName)
        {
            foreach (string[] regionData in regionDataList)
            {
                if (regionData[0] == regionName)
                {
                    string data = regionData[1];

                    string[] lines = data.Split('\n');

                    foreach (string line in lines)
                    {
                        if (line.StartsWith(variableName))
                        {
                            return line.Remove(0, variableName.Length + 2);
                        }
                    }
                }
            }
            return null;
        }
    }
}
