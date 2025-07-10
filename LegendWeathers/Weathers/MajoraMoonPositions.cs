using LegendWeathers.Utils;
using System.Collections.Generic;
using System.Linq;
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
            Vector3 position = default;
            EntranceTeleport? mainEntrance = null;
            var teleports = Object.FindObjectsOfType<EntranceTeleport>();
            if (teleports != null && teleports.Length != 0)
            {
                mainEntrance = teleports.ToList().Find(e => e != null && e.enabled && e.isEntranceToBuilding && e.entranceId == 0);
                if (mainEntrance != null)
                    position = Effects.GetClosestAINodePosition(RoundManager.Instance.outsideAINodes, Vector3.Lerp(StartOfRound.Instance.middleOfShipNode.transform.position, mainEntrance.transform.position, 0.5f));
            }
            if (mainEntrance == null)
                position = RoundManager.Instance.outsideAINodes[RoundManager.Instance.outsideAINodes.Length / 2].transform.position;
            return (position + Vector3.up * 300f, new Vector3(45, 90, 0), position);
        }

        // MoonName, (StartPosition, StartRotation, EndPosition)
        private static readonly Dictionary<string, (Vector3, Vector3, Vector3)> positionInfo = new Dictionary<string, (Vector3, Vector3, Vector3)>
        {
            { "Experimentation", (new Vector3(-44, 300, 85), new Vector3(45, 180, 0), new Vector3(-46, 0, -14)) },
            { "Assurance", (new Vector3(25, 300, -50), new Vector3(45, 0, 0), new Vector3(32, 0, -40)) },
            { "Vow", (new Vector3(-186, 300, 100), new Vector3(45, 90, 0), new Vector3(-4, -1, 26)) },
            { "March", (new Vector3(-120, 300, 0), new Vector3(45, 90, 0), new Vector3(-65, -10, -9)) },
            { "Offense", (new Vector3(150, 300, -100), new Vector3(45, 270, 0), new Vector3(46, 6, -103)) },
            { "Adamance", (new Vector3(-200, 250, 100), new Vector3(45, 120, 0), new Vector3(-119, 10, 33)) },
            { "Rend", (new Vector3(100, 290, -80), new Vector3(45, 315, 0), new Vector3(3, -10, -92)) },
            { "Dine", (new Vector3(-100, 290, -100), new Vector3(45, 0, 0), new Vector3(-96, -10, -20)) },
            { "Titan", (new Vector3(50, 330, -50), new Vector3(45, 290, 0), new Vector3(-17, 37, -6)) },
            { "Artifice", (new Vector3(-100, 300, -100), new Vector3(45, 90, 0), new Vector3(34, 3, -71)) },
            { "Embrion", (new Vector3(-100, 250, -100), new Vector3(45, 0, 0), new Vector3(-87, -5, -21)) },
            { "Atlas Abyss", (new Vector3(-120, 150, -30), new Vector3(45, 90, 0), new Vector3(-80, 10, -24)) },
            { "Bozoros", (new Vector3(-100, 300, -100), new Vector3(45, 50, 0), new Vector3(-66, 0, -14)) },
            { "Desolation", (new Vector3(-77, 298, 75), new Vector3(45, 180, 0), new Vector3(-59, 0, 56)) },
            { "Infernis", (new Vector3(-100, 297, -92), new Vector3(45, 90, 0), new Vector3(-48, 9, -72)) },
            { "Solace", (new Vector3(0, 300, 150), new Vector3(45, 180, 0), new Vector3(-10, -4, 45)) },
            { "StarlancerZero", (new Vector3(-50, 300, -150), new Vector3(45, 0, 0), new Vector3(-66, 0, -120)) },
            { "Synthesis", (new Vector3(-100, 300, -10), new Vector3(45, 90, 0), new Vector3(-100, 0, -14)) },
            { "Arcadia", (new Vector3(-200, 295, -50), new Vector3(45, 90, 0), new Vector3(-115, 0, -26)) },
            { "Acidir", (new Vector3(-60, 230, 60), new Vector3(45, 90, 0), new Vector3(-48, 0, 23)) },
            { "Alcatras", (new Vector3(-150, 300, 100), new Vector3(45, 120, 0), new Vector3(-75, -3, 35)) },
            { "Asteroid-13", (new Vector3(-150, 300, -50), new Vector3(45, 90, 0), new Vector3(-126, -5, -63)) },
            { "Hyve", (new Vector3(-100, 300, 0), new Vector3(45, 90, 0), new Vector3(-77, 2, -10)) },
            { "Atlantica", (new Vector3(-100, 298, 0), new Vector3(45, 90, 0), new Vector3(-73, 0, 7)) },
            { "Calist", (new Vector3(66, 305, -80), new Vector3(45, 0, 0), new Vector3(37, 1, -52)) },
            { "Demetrica", (new Vector3(-100, 298, -11), new Vector3(45, 90, 0), new Vector3(-58, 1, 10)) },
            { "Empra", (new Vector3(-1300, 400, -17), new Vector3(45, 90, 0), new Vector3(-1298, 0, 0)) },
            { "Etern", (new Vector3(-50, 300, -50), new Vector3(45, 90, 0), new Vector3(-66, 0, -52)) },
            { "Filitrios", (new Vector3(43, 299, 53), new Vector3(45, 180, 0), new Vector3(11, 0, 21)) },
            { "Fission-C", (new Vector3(-34, 298, -90), new Vector3(45, 0, 0), new Vector3(-67, 5, -82)) },
            { "Gloom", (new Vector3(-100, 150, -10), new Vector3(45, 90, 0), new Vector3(-87, -5, -9)) },
            { "Gratar", (new Vector3(70, 300, -90), new Vector3(45, 0, 0), new Vector3(62, 2, -85)) },
            { "Junic", (new Vector3(131, 298, 0), new Vector3(45, 270, 0), new Vector3(99, 0, -7)) },
            { "Lecaro", (new Vector3(-50, 300, -80), new Vector3(45, 90, 0), new Vector3(-47, 5, -90)) },
            { "Motra", (new Vector3(51, 328, 13), new Vector3(45, 180, 0), new Vector3(48, 30, 33)) },
            { "Oldred", (new Vector3(37, 305, 103), new Vector3(45, 180, 0), new Vector3(33, 4, 101)) },
            { "Polarus", (new Vector3(-80, 298, -80), new Vector3(45, 45, 0), new Vector3(-79, -30, -74)) },
            { "Utril", (new Vector3(30, 298, 41), new Vector3(45, 180, 0), new Vector3(11, 12, 15)) },
            { "Baykal", (new Vector3(-35, 295, -100), new Vector3(45, 0, 0), new Vector3(-32, -3, -64)) },
            { "Flicker", (new Vector3(11, 300, -170), new Vector3(45, 0, 0), new Vector3(4, -1, -147)) },
            { "The Iris", (new Vector3(150, 300, 80), new Vector3(45, 250, 0), new Vector3(-25, 0, 58)) },
            { "Halation", (new Vector3(31, 298, 30), new Vector3(45, 270, 0), new Vector3(33, -5, 2)) },
            { "Lament", (new Vector3(-177, 300, 47), new Vector3(45, 90, 0), new Vector3(-177, -25, 47)) },
            { "Lithium", (new Vector3(-37, 300, -132), new Vector3(45, 330, 0), new Vector3(-37, 0, -132)) },
            { "Mazon", (new Vector3(-41, 300, 98), new Vector3(45, 90, 0), new Vector3(-41, 0, 98)) },
            { "Kaleidos", (new Vector3(177, 300, 39), new Vector3(45, 220, 0), new Vector3(177, 0, 39)) },
            { "Pandoramus", (new Vector3(-92, 300, 57), new Vector3(45, 150, 0), new Vector3(-92, 0, 57)) },
            { "Pareidolia", (new Vector3(75, 300, -149), new Vector3(45, 270, 0), new Vector3(75, 0, -149)) },
            { "Praetor", (new Vector3(-120, 150, -30), new Vector3(45, 90, 0), new Vector3(-58, 0, -25)) },
            { "Rockwell", (new Vector3(290, 297, 110), new Vector3(45, 180, 0), new Vector3(302, 0, 103)) },
            { "Ganimedes", (new Vector3(53, 300, 33), new Vector3(45, 180, 0), new Vector3(53, 11, 33)) },
            { "Prominence", (new Vector3(-55, 300, 103), new Vector3(45, 100, 0), new Vector3(-55, -9, 103)) },
            { "Sierra", (new Vector3(17, 300, -41), new Vector3(45, 0, 0), new Vector3(17, 18, -41)) },
            { "Temper", (new Vector3(-162, 300, -76), new Vector3(45, 90, 0), new Vector3(-162, -8, -76)) },
            { "Kast", (new Vector3(-39, 295, 30), new Vector3(45, 150, 0), new Vector3(-39, -1, 30)) },
            { "Secret Labs", (new Vector3(100, 300, 0), new Vector3(45, 290, 0), new Vector3(60, 0, 7)) },
            { "Sector-0", (new Vector3(60, 300, -60), new Vector3(45, 320, 0), new Vector3(20, -2, -64)) },
            { "Aquatis", (new Vector3(-83, 300, -12), new Vector3(45, 60, 0), new Vector3(-83, -2, -12)) },
            { "Orion", (new Vector3(-74, 299, 55), new Vector3(45, 180, 0), new Vector3(-74, 1, 55)) },
            { "Wither", (new Vector3(-60, 330, -100), new Vector3(45, 0, 0), new Vector3(-39, 5, -110)) },
            { "Bilge", (new Vector3(-117, 297, -23), new Vector3(45, 130, 0), new Vector3(-117, -20, -23)) },
            { "Chronos", (new Vector3(-37, 300, -78), new Vector3(45, 75, 0), new Vector3(-37, -3, -78)) },
            { "Acheron", (new Vector3(-67, 295, 56), new Vector3(45, 160, 0), new Vector3(-67, -20, 56)) },
            { "-Acheron", (new Vector3(-67, 295, 56), new Vector3(45, 160, 0), new Vector3(-67, -20, 56)) },
            { "Kanie", (new Vector3(-220, 300, -16), new Vector3(45, 50, 0), new Vector3(-220, 1, -16)) },
            { "EGypt", (new Vector3(-115, 300, -30), new Vector3(45, 45, 0), new Vector3(-119, 0, -33)) },
            { "Noctis", (new Vector3(-54, 300, 40), new Vector3(45, 150, 0), new Vector3(-54, -2, 40)) },
            { "Luigis Mansion", (new Vector3(-99, 303, 17), new Vector3(45, 50, 0), new Vector3(-99, 0, 17)) }
        };
    }
}
