using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Xml;

namespace Assembly_CSharp.TasInfo.mm.Source {
    internal class SplitReader { 
        public static List<SplitClass> SplitList = new List<SplitClass>();
        public static void OnInit() {

            XmlTextReader reader = new XmlTextReader(ConfigManager.SplitFileLocation);

            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            XmlNodeList segment_Names = doc.SelectNodes("Run/Segments/Segment/Name");
            XmlNodeList segment_Tirggers = doc.SelectNodes("Run/AutoSplitterSettings/Splits/Split");

            List<SplitName> splitNames = new List<SplitName>();

            foreach (XmlNode node in segment_Tirggers) {
                splitNames.Add((SplitName)Enum.Parse(typeof(SplitName), node.InnerText));
            }

            for (int i = 0; i < segment_Names.Count; i++) {
                if (i >= splitNames.Count) {
                    SplitList.Add(new SplitClass(segment_Names[i].InnerText, SplitName.ManualSplit));
                } else {
                    SplitList.Add(new SplitClass(segment_Names[i].InnerText, splitNames[i]));
                }
            }

            foreach (SplitClass splitClass in SplitList) {
                Console.WriteLine(splitClass.SplitRef.SplitTitle + " " + splitClass.SplitRef.SplitTrigger);
            }
        }
    }
}
