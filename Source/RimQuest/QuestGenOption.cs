using System.Xml;
using RimWorld;
using Verse;

namespace RimQuest
{
    public class QuestGenOption
    {
        public readonly QuestScriptDef def;

        public float selectionWeight;

        public QuestGenOption()
        {
        }

        public QuestGenOption(QuestScriptDef def, float selectionWeight)
        {
            this.def = def;
            this.selectionWeight = selectionWeight;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name);
            selectionWeight = (float) ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
        }
    }
}