using System.Xml;
using RimWorld;
using Verse;

namespace RimQuest;

public class IncidentGenOption
{
    public readonly IncidentDef def;

    public float selectionWeight;

    public IncidentGenOption()
    {
    }

    public IncidentGenOption(IncidentDef def, float selectionWeight)
    {
        this.def = def;
        this.selectionWeight = selectionWeight;
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name);
        selectionWeight = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
    }
}