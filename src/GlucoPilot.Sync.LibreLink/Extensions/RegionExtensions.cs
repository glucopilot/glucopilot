using System;
using GlucoPilot.Data.Enums;
using GlucoPilot.LibreLinkClient;

namespace GlucoPilot.Sync.LibreLink.Extensions;

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
                Region.Eu2 => LibreRegion.Eu2,
                Region.Fr => LibreRegion.Fr,
                Region.Jp => LibreRegion.Jp,
                Region.Us => LibreRegion.Us,
                _ => throw new ArgumentException("Invalid region", nameof(region))
            };
        }
    }

    extension(LibreRegion libreRegion)
    {
        public Region ToRegion()
        {
            return libreRegion switch
            {
                LibreRegion.Ae => Region.Ae,
                LibreRegion.Ap => Region.Ap,
                LibreRegion.Au => Region.Au,
                LibreRegion.Ca => Region.Ca,
                LibreRegion.De => Region.De,
                LibreRegion.Eu => Region.Eu,
                LibreRegion.Eu2 => Region.Eu2,
                LibreRegion.Fr => Region.Fr,
                LibreRegion.Jp => Region.Jp,
                LibreRegion.Us => Region.Us,
                _ => throw new ArgumentException("Invalid libreRegion", nameof(libreRegion))
            };
        }
    }
}