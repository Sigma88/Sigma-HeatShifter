using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.Configuration.Parsing;


namespace SigmaHeatShifterPlugin
{
    [ParserTargetExternal("Body", "Atmosphere", "Kopernicus")]
    internal class AtmosphereSettingsLoader : BaseLoader
    {
        [ParserTarget("maxTempAngleOffset", Optional = true)]
        NumericParser<float> MaxTempAngleOffset
        {
            set
            {
                generatedBody.celestialBody.Set("maxTempAngleOffset", value.Value);
            }
        }
    }
}
