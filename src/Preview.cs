using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    public class PreviewSegment {

	// Fields
	
	public Orbit orbit0;
	public Orbit orbitf;
	public Orbit[] orbits;
	public LineRenderer line;
	public double UT0;
	public double UTf;
	double dT;
	GameObject obj;
	public VectorLine vline;

	// Constructor & calculate

	public PreviewSegment(ModuleSolarSail sail, Orbit orbitInitial, double UT0, double UTf, SailControl control) {
	    
	    this.UT0 = UT0;
	    Debug.Log("UT0: " + UT0.ToString());
	    this.UTf = UTf;
	    Debug.Log("UTf: " + UTf.ToString());
	    dT = TimeWarp.fixedDeltaTime * control.factor;
	    Debug.Log("dT: " + dT.ToString());
	    
	    // Calculate preview orbits

	    orbits = ModuleSolarSail.PropagateOrbit(sail, orbitInitial, UT0, UTf, dT, control.cone, control.clock, sail.vessel.GetTotalMass());
	    orbit0 = orbits[0];
	    orbitf = orbits[orbits.Length - 1];
	    
	    // Initialize LineRenderer

	    // Remove old line
	    if (line != null) {
		UnityEngine.Object.Destroy(line);
	    }
	    // Create new one
	    obj = new GameObject("Preview segment");
	    line = obj.AddComponent<LineRenderer>();
	    line.useWorldSpace = false;
	    obj.layer = 10; // Map view
	    line.material = MapView.fetch.orbitLinesMaterial;
	    line.SetColors(Color.yellow, Color.yellow);
	    line.SetWidth(20000, 20000);
	    line.SetVertexCount(orbits.Length);
	}

	// Update segment during renders

	public void Update(Vessel vessel) {
	    if (line != null) {
		// Enable only on map
		if (MapView.MapIsEnabled) {
		    line.enabled = true;
		    // Update points
		    Vector3d rRefUT0 = vessel.orbit.referenceBody.getPositionAtUT(UT0);
		    for (int i = 0; i < orbits.Length; i++) {
			double UTi = orbits[i].epoch;
			line.SetPosition(i, ScaledSpace.LocalToScaledSpace(orbits[i].getRelativePositionAtUT(UTi).xzy + rRefUT0));
		    }
		} else {
		    line.enabled = false;
		}
	    }
	}
    }
    
    public class Preview {
	
	// Fields
	PreviewSegment[] segments; // Trajectory segments
	ModuleSolarSail sail; // Sail this preview is attached to
	LineRenderer linef; // Final orbit line
	Vector3d[] linefPoints; // 3d points of final orbit
	
	// Constructor
	
	public Preview(ModuleSolarSail sail) {
	    this.sail = sail;
	}

	// Calculate & draw trajectory prediction
	
	public void Calculate () {
	    // Destroy existing lines
	    if (segments != null) {
		foreach(var segment in segments) {
		    if (segment.line != null) {
			UnityEngine.Object.Destroy(segment.line);
		    }
		}
	    }
	    // New segments array
	    segments = new PreviewSegment[sail.controls.ncontrols];
	    // Beginning time
	    double UT0 = sail.controls.UT0;
	    Orbit orbitInitial = sail.vessel.orbit;
	    // Calculate each segment
	    for (var i = 0; i < segments.Length; i++) {
		// End time
		double UTf = UT0 + sail.controls.controls[i].duration;
		// Calculate segment
		segments[i] = new PreviewSegment(sail, orbitInitial, UT0, UTf, sail.controls.controls[i]);
		// Update initial time
		UT0 = UTf;
		// Update initial orbit
		orbitInitial = segments[i].orbitf;
	    }

	    // Draw one complete final orbit

	    // Destroy existing line
	    if (linef != null) {
		UnityEngine.Object.Destroy(linef);
	    }
	    // Create linerenderer & object
	    GameObject objf = new GameObject("Final orbit");
	    linef = objf.AddComponent<LineRenderer>();
	    linef.useWorldSpace = false;
	    objf.layer = 10; // Map
	    linef.material = MapView.fetch.orbitLinesMaterial;
	    linef.SetColors(Color.cyan, Color.cyan);
	    linef.SetWidth(20000, 20000);
	    linef.SetVertexCount(360);
	    // 3D points to use in linef
	    linefPoints = new Vector3d[360];
	    // Final orbit of sail trajectory
	    Orbit orbitf = orbitInitial;
	    // Period of final orbit
	    double TPf = orbitf.period;
	    // Position of reference body at end of trajectory
	    Vector3d rRefUTf = orbitf.referenceBody.getPositionAtUT(UT0);
	    // Populate points
	    for(var i = 0; i < 360; i++) {
		double UTi = UT0 + i * TPf / 360;
		// Relative orbitf position
		Vector3d rRelOrbitf = orbitf.getRelativePositionAtUT(UTi).xzy;
		// Absolute position
		linefPoints[i] = rRefUTf + rRelOrbitf;
	    }
	}
	

	// Update
	public void Update (Vessel vessel) {
	    foreach(var segment in segments) {
		segment.Update(vessel);
	    }

	    // Update final orbit line from points
	    if (linef != null) {
		if (MapView.MapIsEnabled) {
		    linef.enabled = true;
		    for (var i = 0; i < 360; i++) {
			linef.SetPosition(i, ScaledSpace.LocalToScaledSpace(linefPoints[i]));
		    }
		} else {
		    linef.enabled = false;
		}
	    }
	}
    }
}
