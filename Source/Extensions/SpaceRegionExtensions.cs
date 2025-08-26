using System.Linq;
using Game;
using Game.Systems.Space;

namespace Infiniverse.Extensions;

public static class SpaceRegionExtensions
{
    public static void ClearReferences(this SpaceRegion region)
    {
        A.S.Query.GetSpaceMap.Ask().Regions.Remove(region);
        region.SO.ClearReferences();
        foreach (var link in region.Links.ToList())
        {
            var other = link.To as SpaceRegion;
            other?.Links.Remove(link);
        }
    }
}