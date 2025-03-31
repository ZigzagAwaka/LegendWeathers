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
            var position = RoundManager.Instance.outsideAINodes[RoundManager.Instance.outsideAINodes.Length / 2].transform.position;
            return (position + Vector3.up * 300f, new Vector3(45, 0, 0), position);
        }

        // MoonName, (StartPosition, StartRotation, EndPosition)
        private static readonly Dictionary<string, (Vector3, Vector3, Vector3)> positionInfo = new Dictionary<string, (Vector3, Vector3, Vector3)>
        {
            { "Experimentation", (new Vector3(-44, 300, 85), new Vector3(45, 180, 0), new Vector3(-46, 0, -14)) },
            { "Vow", (new Vector3(-186, 300, 100), new Vector3(45, 90, 0), new Vector3(-4, -1, 26)) },
            { "March", (new Vector3(-320, 100, 0), new Vector3(20, 90, 0), new Vector3(-48, -3, -39)) },
            { "Offense", (new Vector3(150, 300, -100), new Vector3(45, 270, 0), new Vector3(46, 6, -103)) },
            { "Adamance", (new Vector3(-50, 200, 150), new Vector3(45, 180, 0), new Vector3(-60, 2, -9)) },
            { "Rend", (new Vector3(100, 290, -80), new Vector3(45, 315, 0), new Vector3(3, -10, -92)) },
            { "Dine", (new Vector3(-100, 290, -100), new Vector3(45, 0, 0), new Vector3(-96, -10, -20)) },
            { "Titan", (new Vector3(50, 330, -50), new Vector3(45, 290, 0), new Vector3(-17, 37, -6)) },
            { "Artifice", (new Vector3(-100, 300, -100), new Vector3(45, 90, 0), new Vector3(34, 3, -71)) },
            { "Embrion", (new Vector3(-100, 250, -100), new Vector3(45, 0, 0), new Vector3(-87, -5, -21)) }
        };
    }
}
