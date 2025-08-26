using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Data.Space;
using Game.Systems.Space;
using Game.UI;
using HarmonyLib;
using Multiplayer.Misc;
using UnityEngine;

namespace Multiplayer.Extensions;

public static class SpaceObjectExtensions
{
    public static void ClearReferences(this SpaceObject so)
    {
        so.S.Universe.ObjectsById.Remove(so.Id);
        so.Parent.Children.Remove(so);
            
        if (A.Starmap != null)
        {
            A.Starmap.RemoveObject(so);
        }

        if (so.Children != null)
        {
            foreach (var child in so.Children.ToList())
            {
                ClearReferences(child);
            }
        }
    }
}