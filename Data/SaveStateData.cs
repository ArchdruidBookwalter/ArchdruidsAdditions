using System;
using System.Collections.Generic;
using System.Linq;
using static ArchdruidsAdditions.Data.SaveStateData.SaveStateDataContainer;

namespace ArchdruidsAdditions.Data;

public static class SaveStateData
{
    public static Dictionary<string, SaveStateDataContainer> saveStateDataContainers = [];

    public static SaveState parasiteSaveState;

    public class SaveStateDataContainer
    {
        public string modName;

        public Dictionary<SlugcatStats.Name, CampaignDataContainer> campaignDataValues = [];
        public Dictionary<string, DataValue> baseValues = [];

        public SaveStateDataContainer(string modName)
        {
            this.modName = modName;

            saveStateDataContainers.Add(modName, this);
        }

        public virtual void InitializeValuesForCampaign(SlugcatStats.Name name)
        {
            if (!campaignDataValues.ContainsKey(name))
            {
                CampaignDataContainer newContainer = new(name, baseValues);
                campaignDataValues.Add(name, newContainer);
            }
        }

        public class CampaignDataContainer
        {
            public SlugcatStats.Name slugName;
            public List<DataValue> values;

            public CampaignDataContainer(SlugcatStats.Name slugName, Dictionary<string, DataValue> baseValues)
            {
                this.slugName = slugName;
                values = [];

                foreach (string key in baseValues.Keys)
                {
                    values.Add(new DataValue(key, baseValues[key].valueType));
                }
            }

            public DataValue GetValue(string name)
            {
                foreach (DataValue value in values)
                {
                    if (value.name == name)
                    { return value; }
                }
                return null;
            }

            public void SetValue(string name, object newValue)
            {
                foreach (DataValue value in values)
                {
                    if (value.name == name && value.valueType == newValue.GetType())
                    { value.value = newValue; return; }
                }
            }

            public bool HasValue(string name)
            {
                foreach (DataValue value in values)
                {
                    if (value.name == name)
                    { return true; }
                }
                return false;
            }
        }

        public class DataValue
        {
            public DataValue(string name, Type valueType)
            {
                this.name = name;
                this.valueType = valueType;
            }

            public string name;
            public Type valueType;

            public object value;

            public static object GenericDataValueFromString(string value, Type type)
            {
                if (type == typeof(int))
                {
                    return int.Parse(value);
                }
                if (type == typeof(bool))
                {
                    return bool.Parse(value);
                }
                if (type == typeof(float))
                {
                    return float.Parse(value);
                }
                if (type == typeof(EntityID))
                {
                    return EntityID.FromString(value);
                }
                return null;
            }
        }

        public int GetInt(string valueName, SlugcatStats.Name slugName)
        {
            if (!campaignDataValues.ContainsKey(slugName))
            { InitializeValuesForCampaign(slugName); }

            return (int)campaignDataValues[slugName].GetValue(valueName).value;
        }
        public float GetFloat(string valueName, SlugcatStats.Name slugName)
        {
            if (!campaignDataValues.ContainsKey(slugName))
            { InitializeValuesForCampaign(slugName); }

            return (float)campaignDataValues[slugName].GetValue(valueName).value;
        }
        public bool GetBool(string valueName, SlugcatStats.Name slugName)
        {
            if (!campaignDataValues.ContainsKey(slugName))
            { InitializeValuesForCampaign(slugName); }

            return (bool)campaignDataValues[slugName].GetValue(valueName).value;
        }
        public EntityID GetEntityID(string valueName, SlugcatStats.Name slugName)
        {
            if (!campaignDataValues.ContainsKey(slugName))
            { InitializeValuesForCampaign(slugName); }

            return (EntityID)campaignDataValues[slugName].GetValue(valueName).value;
        }

        public void SetValue(string valueName, SlugcatStats.Name slugName, object newValue)
        {
            if (!campaignDataValues.ContainsKey(slugName))
            { InitializeValuesForCampaign(slugName); }

            campaignDataValues[slugName].SetValue(valueName, newValue);
        }
    }

    public static AASaveStateDataContainer AASaveDataContainer = new();

    public class AASaveStateDataContainer : SaveStateDataContainer
    {
        public AASaveStateDataContainer() : base(Plugin.PLUGIN_GUID)
        { }

        public override void InitializeValuesForCampaign(SlugcatStats.Name slugName)
        {
            baseValues.Add("INFECTED", new DataValue("INFECTED", typeof(bool)));

            base.InitializeValuesForCampaign(slugName);
        }

        public bool GetInfected(SlugcatStats.Name campaign)
        { return GetBool("INFECTED", campaign); }
        public void SetInfected(SlugcatStats.Name campaign, bool newInfected)
        { SetValue("INFECTED", campaign, newInfected); }
    }
}
