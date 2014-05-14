﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wosh.logic
{
    /*
     * 
     * String content = new System.Net.WebClient().DownloadString(@"http://augo/go/cctray.xml");
     * List<MetaData> data = XmlParser.ParseString(content);
     * foreach (MetaData d in data) {
     *      // Do Something Here
     * }
     * */
    public class XmlParser
    {
        // A list of the projects names to be excluded from display.
        public static List<String> ExcludedGroupProjects = new List<string>();
        public static List<String> ExcludedIndividualProjects = new List<string>();
        public static bool ShouldExcludeProjects;

        static public List<MetaData> ParseString(String input)
        {
            XmlReader reader = XmlReader.Create(new System.IO.StringReader(input));
            List<MetaData> list = new List<MetaData>();

            reader.ReadToFollowing("Project");

            while (!reader.EOF)
            {
                MetaData data = new MetaData();

                if (reader.MoveToAttribute("name")) data.Name = reader.Value;
                if (reader.MoveToAttribute("activity")) data.Activity = reader.Value;
                if (reader.MoveToAttribute("lastBuildStatus")) data.LastBuildStatus = reader.Value;
                if (reader.MoveToAttribute("lastBuildLabel")) data.LastBuildLabel = reader.Value;
                if (reader.MoveToAttribute("lastBuildTime")) data.LastBuildTime = reader.Value;
                if (reader.MoveToAttribute("webUrl")) data.WebUrl = reader.Value;

                // Set the group name, the stage, and the job.
                String[] splitName = data.Name.Split(new[] {":", ":"}, StringSplitOptions.RemoveEmptyEntries);
                data.GroupName = splitName[0].Trim();
                data.Stage = splitName.Length >= 2 ? splitName[1].Trim() : String.Empty;
                data.Job = splitName.Length >= 3 ? splitName[2].Trim() : String.Empty;
                // If the project name is in the excluded indiviual projects, don't add it to the ouput list.
                if (!ExcludedIndividualProjects.Contains(data.Name)) list.Add(data);
                reader.ReadToFollowing("Project");
            }
            return list;
        }

        static public List<GroupedMetaData> ParseStringForGroup(String input)
        {
            // Obtain the list of meta data from the other class method.
            List<MetaData> metaData = ParseString(input);
            // Create a dictornary to store the grouped data in.
            Dictionary<String, GroupedMetaData> groupData = new Dictionary<String, GroupedMetaData>();
            /*
             * Loop throuh all the metadata.
             * 
             * If there is a group with the same "prefix" (E.g "Support :: Build", prefix is "Support")
             *      Add it the the group.
             * If there isn't, create a new grouped metadata object to store it in.
             * 
             * If the group name (the prefix) is in the excluded group projects class varable, we won't add it to the dictonary.
             */
            foreach (MetaData data in metaData)
            {
                GroupedMetaData value;
                String groupName = data.GroupName;
                // Pass, because we don't want to add this data to the output.
                if (ExcludedGroupProjects.Contains(groupName)) continue;

                // Look for the group, if it isn't there, create it.
                if (!groupData.TryGetValue(groupName, out value)) {
                    // No grouped data, must create our own.
                    value = new GroupedMetaData();
                    value.Name = groupName;
                    value.SubData = new List<MetaData>();
                    groupData.Add(groupName, value);
                }
                // Add the metadata to the group data's subdata.
                value.SubData.Add(data);
            }

            return groupData.Values.ToList();
        }
    }
}
