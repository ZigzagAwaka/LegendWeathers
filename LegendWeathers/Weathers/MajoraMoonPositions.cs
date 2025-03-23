using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LegendWeathers.Weathers
{
    public static class MajoraMoonPositions
    {
        public static (Vector3, Vector3, Vector3) Get(string planetName, bool searchKnown = true)
        {
            if (searchKnown)
            {
                var moonName = Regex.Replace(planetName, "^[0-9]+", string.Empty);
                if (moonName[0] == ' ')
                    moonName = moonName[1..];
                if (positionInfo.ContainsKey(moonName))
                    return positionInfo[moonName];
            }
            var position = RoundManager.Instance.outsideAINodes[^1].transform.position;
            return (position + Vector3.up * 200f, new Vector3(45, 0, 0), position);
        }

        // MoonName, (StartPosition, StartRotation, EndPosition)
        private static readonly Dictionary<string, (Vector3, Vector3, Vector3)> positionInfo = new Dictionary<string, (Vector3, Vector3, Vector3)>
        {
            { "Experimentation", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) },
            { "Assurance", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) },
            { "Vow", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) },
            { "March", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) },
            { "Offense", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) },
            { "Adamance", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) },
            { "Rend", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) },
            { "Dine", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) },
            { "Titan", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) },
            { "Artifice", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) },
            { "Embrion", (new Vector3(0,0,0), new Vector3(0,0,0), new Vector3(0,0,0)) }
        };
    }
}
