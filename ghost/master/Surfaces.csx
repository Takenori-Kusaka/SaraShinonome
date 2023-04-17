#load "SurfaceCategory.csx"
using System;
using System.Collections.Generic;

public class Surfaces
{
    static Random random = new Random();
    static Dictionary<string, Surfaces> SurfaceList = new Dictionary<string, Surfaces>()
    {
        [SurfaceCategory.Normal] = new Surfaces(0),
        [SurfaceCategory.Thinking] = new Surfaces(1),
        [SurfaceCategory.Surprise] = new Surfaces(2),
        [SurfaceCategory.Sad] = new Surfaces(3),
        [SurfaceCategory.Amazed] = new Surfaces(4),
        [SurfaceCategory.Smile] = new Surfaces(5),
        [SurfaceCategory.Bashful] = new Surfaces(6),
        [SurfaceCategory.Excited] = new Surfaces(7),
    };

    public static Surfaces Of(string category) => SurfaceList[category];

    int[] surfaces;
    public Surfaces(params int[] surfaces)
    {
        this.surfaces = surfaces;
    }
    public int GetRaodomSurface()
    {
        return surfaces[random.Next(surfaces.Length)];
    }
    public int GetSurfaceFromRate(double rate)
    {
        var index = Math.Min((int)(surfaces.Length * rate), surfaces.Length - 1);
        return surfaces[index];
    }

}