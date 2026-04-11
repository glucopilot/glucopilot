using System;
using GlucoPilot.Data.Enums;
using GlucoPilot.LibreLinkClient;

namespace GlucoPilot.Api.Extensions;

public static class RegionExtensions
{
    extension(Region region)
    {
        public LibreRegion ToLibreRegion()
        {
            return region switch
            {
                Region.Ae => LibreRegion.Ae,
                Region.Ap => LibreRegion.Ap,
                Region.Au => LibreRegion.Au,
                Region.Ca => LibreRegion.Ca,
                Region.De => LibreRegion.De,
                Region.Eu => LibreRegion.Eu,
                Region.Fr => LibreRegion.Fr,
                Region.Jp => LibreRegion.Jp,
                Region.Us => LibreRegion.Us,
                _ => throw new ArgumentException("Invalid region", nameof(region))
            };
        }
    }
}