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
	
	public static Quaternion SailFrameLocal (float cone, float clock, float flatspin) {
	    // Unit vectors (in Unity coordinate system)
	    var r = new Vector3d(0, -1, 0); // radial
	    //var t = new Vector3d(0, 0, 1); // tangential
	    var n = new Vector3d(1, 0, 0); // orbit normal

	    // Clock angle rotation
	    var Qclock = Quaternion.AngleAxis(clock, r);
	    var rclock = Qclock * r;
	    //var tclock = Qclock * t;
	    var nclock = Qclock * n;

	    // Cone angle rotation
	    var Qcone = Quaternion.AngleAxis(cone, nclock);
	    var rcone = Qcone * rclock;
	    //var tcone = Qcone * tclock;
	    //var ncone = Qcone * nclock;
	    
	    // Flatspin rotation
	    var Qfs = Quaternion.AngleAxis(flatspin, rcone);

	    // Total quaternion
	    return Qfs * Qcone * Qclock;
	}

	// Sail frame given an orbit, angles, and UT
	public static Quaternion SailFrame (Orbit orbit, float cone, float clock, float flatspin, double UT) {
	    var QRTN = RTNFrame(orbit, UT);
	    var QSL = SailFrameLocal(cone, clock, flatspin);
	    return QRTN * QSL;
	}
    }
}
