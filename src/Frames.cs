using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    public class Frame {

	// Delagates
	// Base frame quaternion given orbit and UT
	public delegate Quaternion Qbase (Orbit orbit, double UT);
	// Local frame given angles
	public delegate Quaternion Qlocal (float[] angles);
	// Quaternion of vessel given orbit, UT, and angles
	public delegate Quaternion Q (Orbit orbit, double UT, float[] angles);
	
	// Fields
	public string name;
	public string[] names;
	public float[] defaults;
	public Qbase qbasefn;
	public Qlocal qlocalfn;
	public Q qfn;
	
	// Constructor
	public Frame (string name, string[] names, float[] defaults, Qbase qbasefn, Qlocal qlocalfn, Q qfn) {
	    this.name = name;
	    this.names = names;
	    this.defaults = defaults;
	    this.qbasefn = qbasefn;
	    this.qlocalfn = qlocalfn;
	    this.qfn = qfn;
	}

	public static Dictionary<string, Frame> Frames = new Dictionary<string, Frame>
	    {
		// Radial/tangential/normal reference frame
		// With Cone/Clock/Flatspin angles
		{"RTN", new Frame("RTN",
				  new string[] {"Cone", "Clock", "Flatspin"},
				  new float[] {90.0f, 0.0f, 0.0f},
				  RTNFrame,
				  SailFrameLocal,
				  SailFrame)},
		// In-track/cross-track/normal reference frame
		// With Flight path angle (FPA)/Azimuth/Flatspin angles
		{"ICN", new Frame("ICN",
				  new string[] {"FPA", "Az", "Flatspin"},
				  new float[] {0f, 0f, 0f},
				  ICNFrame,
				  FAFLocal,
				  FAFFrame)}
	    };

	// Quaternion functions used by reference frames

	// RTN Frame
	
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
	
	public static Quaternion SailFrameLocal (float[] angles) {
	    // Angles
	    var cone = angles[0];
	    var clock = angles[1];
	    var flatspin = angles[2];
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
	public static Quaternion SailFrame (Orbit orbit, double UT, float[] angles) {
	    var QRTN = RTNFrame(orbit, UT);
	    var QSL = SailFrameLocal(angles);
	    return QRTN * QSL;
	}

	// In-track/Cross-track/normal frame

	// Calculate ICN frame from orbit and universal time
	public static Quaternion ICNFrame (Orbit orbit, double UT) {
	    // Unit position
	    var r = orbit.getRelativePositionAtUT(UT).normalized.xzy;
	    // Unit velocity
	    var v = orbit.getOrbitalVelocityAtUT(UT).normalized.xzy;
	    // Unit orbit angular momentum
	    var h = Vector3d.Cross(r, v).normalized;
	    // Cross track unit vector
	    var c = Vector3d.Cross(h, v).normalized;
	    // QICN
	    return Quaternion.LookRotation(c,v);
	}

	// Local frame given angles
	public static Quaternion FAFLocal (float[] angles) {
	    // Angles
	    var FPA = angles[0];
	    var Az = angles[1];
	    var Flatspin = angles[2];

	    // Unit vectors (in Unity coordinate system)
	    var v = new Vector3d(0, 1, 0); // velocity
	    //var c = new Vector3d(0, 0, 1); // cross track
	    var h = new Vector3d(1, 0, 0); // orbit angular momentum

	    // Azimuth rotation about velocity
	    var Qaz = Quaternion.AngleAxis(Az, v);
	    var vAz = Qaz * v;
	    //var cAz = Qaz * c;
	    var hAz = Qaz * h;
	    
	    // Flight path angle rotation
	    var Qfpa = Quaternion.AngleAxis(FPA, hAz);
	    var vFPA = Qfpa * vAz;
	    //var cFPA = Qfpa * cAz;
	    //var hFPA = Qfpa * hAz;

	    // Flatspin rotation
	    var Qfs = Quaternion.AngleAxis(Flatspin, vFPA);

	    // Total quaternion
	    return Qfs * Qfpa * Qaz;
	}

	// Total quaternion
	public static Quaternion FAFFrame (Orbit orbit, double UT, float[] angles) {
	    return ICNFrame(orbit, UT) * FAFLocal(angles);
	}
    }
}
