using System.Collections.Generic;
using UnityEngine;

public static class RaceResultStore
{
    public struct Entry
    {
        public string name;
        public float timeSeconds; // finish time
    }

    public static List<Entry> FinalOrder { get; private set; }
    public static float TargetDistanceMeters { get; private set; }

    public static void SetResults(List<Entry> order, float targetDistanceMeters)
    {
        FinalOrder = order;
        TargetDistanceMeters = targetDistanceMeters;
    }

    public static void Clear()
    {
        FinalOrder = null;
        TargetDistanceMeters = 0f;
    }
}
