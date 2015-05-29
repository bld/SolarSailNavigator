using System;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    public class Frames {

	// Calculate RTN frame quaternion given an orbit and UT
	public static Quaternion RTNFrame (Orbit orbit, double UT) {
	    // Position
	    var r = orbit.getRelativePositionAtUT(UT).normalized.xzy;
	    // Velocity
	    var v = orbit.getOrbitalVelocityAtUT(UT).normalized.xzy;
	    // Unit orbit angular momentum
	    var h = Vector3d.Cross(r, v).normalized;
	    // Tangential
	    var t = Vector3d.Cross(h, r).normalized;
	    // QRTN
	    return Quaternion.LookRotation(t, r);
	}

	// Sail frame in local (RTN) coordinates
	public static Quaternion SailFrameLocal (float cone, float clock) {
	    return Quaternion.Euler(0, 90 - clock, cone);
	}
	
	// Sail frame given an orbit, angles, and UT
	public static Quaternion SailFrame (Orbit orbit, float cone, float clock, double UT) {
	    var QRTN = RTNFrame(orbit, UT);
	    var QCC = SailFrameLocal(cone, clock);
	    return QRTN * QCC;
	}
    }
}
