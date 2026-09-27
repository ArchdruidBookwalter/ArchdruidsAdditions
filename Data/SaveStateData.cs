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

            LogMessage("ADDED NEW SAVESTATEDATA CONTAINER TO DICTIONARY: " + modName);

            saveStateDataContainers.Add(modName, this);
        }

        public void InitalizeForCampaign(SlugcatStats.Name name)
        {
            if (!campaignDataValues.ContainsKey(name))
            {
                if (baseValues.Count == 0)
                { InitializeValuesForCampaign(name); }

                CampaignDataContainer newContainer = new(name, baseValues);
                campaignDataValues.Add(name, newContainer);
            }
        }

        public virtual void InitializeValuesForCampaign(SlugcatStats.Name name)
        {
        }

        public class CampaignDataContainer
        {
            public SlugcatStats.Name slugName;
            public List<DataValue> values;

            public CampaignDataContainer(SlugcatStats.Name slugName, Dictionary<string, DataValue> baseValues)
            {
                LogMessage("INIT CAMPAIGN CONTAINER: " + slugName.value);

                this.slugName = slugName;
                values = [];

                foreach (string key in baseValues.Keys)
                {
                    values.Add(new DataValue(key, baseValues[key].valueType));
                }
            }

            public DataValue GetValue(string name)
            {
                float section = 0;

                try
                {
                    //Debug.Log("");
                    //Debug.Log("SEARCHING FOR VALUE TO GET: " + name);

                    foreach (DataValue value in values)
                    {
                        section = 1;

                        //Debug.Log("FOUND VALUE: " + value.name);

                        if (value.name == name)
                        {
                            //Debug.Log("RETURNED VALUE: " + value.value);
                            return value; 
                        }
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Log_Exception(e, "CAMPAIGNDATACONTAINER_GETVALUE", section);
                    return null;
                }
            }

            public void SetValue(string name, object newValue)
            {
                float section = 0;

                try
                {
                    //Debug.Log("");
                    //Debug.Log("SEARCHING FOR VALUE TO SET: " + name);

                    foreach (DataValue value in values)
                    {
                        //Debug.Log("FOUND VALUE: " + value.name);

                        if (value.name == name && value.valueType == newValue.GetType())
                        {
                            value.value = newValue;
                            return; 
                        }
                    }
                }
                catch (Exception e)
                {
                    Log_Exception(e, "CAMPAIGNDATACONTAINER_SETVALUE", section);
                }
            }

            public bool HasValue(string name)
            {
                float section = 0;

                try
                {
                    foreach (DataValue value in values)
                    {
                        //Debug.Log("FOUND VALUE: " + value.name);

                        if (value.name == name)
                        {
                            //Debug.Log("RETURNED TRUE");
                            return true;
                        }
                    }
                    return false;
                }
                catch (Exception e)
                {
                    Log_Exception(e, "CAMPAIGNDATACONTAINER_HASVALUE", section);

                    return false;
                }
            }
        }

        public class DataValue
        {
            public DataValue(string name, Type valueType)
            {
                this.name = name;
                this.valueType = valueType;

                value = Activator.CreateInstance(valueType);
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
            if (CheckValue(valueName, slugName))
            {
                object value = campaignDataValues[slugName].GetValue(valueName).value;

                if (value is int v)
                { return v; }
            }

            return default;
        }
        public float GetFloat(string valueName, SlugcatStats.Name slugName)
        {
            if (CheckValue(valueName, slugName))
            {
                object value = campaignDataValues[slugName].GetValue(valueName).value;

                if (value is float v)
                {
                    return v;
                }
            }

            return default;
        }
        public bool GetBool(string valueName, SlugcatStats.Name slugName)
        {
            if (CheckValue(valueName, slugName))
            {
                object value = campaignDataValues[slugName].GetValue(valueName).value;

                if (value is bool v)
                { return v; }
            }

            return default;
        }
        public EntityID GetEntityID(string valueName, SlugcatStats.Name slugName)
        {
            if (CheckValue(valueName, slugName))
            {
                object value = campaignDataValues[slugName].GetValue(valueName).value;

                if (value is EntityID v)
                { return v; }
            }

            return default;
        }

        public void SetValue(string valueName, SlugcatStats.Name slugName, object newValue)
        {
            if (CheckValue(valueName, slugName))
            { campaignDataValues[slugName].SetValue(valueName, newValue); }
        }

        public bool CheckValue(string valueName, SlugcatStats.Name slugName)
        {
            if (campaignDataValues.ContainsKey(slugName))
            {
                if (campaignDataValues[slugName].HasValue(valueName))
                {
                    return true;
                }
                Debug.Log("\'CampaignDataValues[slugName]\' DID NOT CONTAIN KEY: " + slugName);
                return false;
            }
            Debug.Log("\'CampaignDataValues\' DID NOT CONTAIN KEY: " + slugName);
            return false;
        }
    }

    public static AASaveStateDataContainer AASaveDataContainer;

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
