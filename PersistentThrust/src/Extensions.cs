using System;
using System.Linq;
using UnityEngine;

namespace PersistentThrust {

    public static class OrbitExtensions {

	// Dublicate an orbit
	public static Orbit Clone(this Orbit orbit0) {
	    return new Orbit(orbit0.inclination, orbit0.eccentricity, orbit0.semiMajorAxis, orbit0.LAN, orbit0.argumentOfPeriapsis, orbit0.meanAnomalyAtEpoch, orbit0.epoch, orbit0.referenceBody);
	}

	// Perturb an orbit by a deltaV vector
	public static void Perturb(this Orbit orbit, Vector3d deltaVV, double UT, double dT) {

	    // Transpose deltaVV Y and Z to match orbit frame
	    Vector3d deltaVV_orbit = deltaVV.xzy;

	    // Position vector
	    Vector3d position = orbit.getRelativePositionAtUT(UT);

	    // Update with current position and new velocity
	    orbit.UpdateFromStateVectors(position, orbit.getOrbitalVelocityAtUT(UT) + deltaVV_orbit, orbit.referenceBody, UT);
	    orbit.Init();
	    orbit.UpdateFromUT(UT);
	}
    }
}
