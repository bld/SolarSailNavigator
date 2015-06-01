using System;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    public static class OrbitExtensions {

	// Dublicate an orbit
	public static Orbit Clone(this Orbit orbit0) {
	    return new Orbit(orbit0.inclination, orbit0.eccentricity, orbit0.semiMajorAxis, orbit0.LAN, orbit0.argumentOfPeriapsis, orbit0.meanAnomalyAtEpoch, orbit0.epoch, orbit0.referenceBody);
	}
    }
}
