using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    public class PreviewSegment {

	// Fields
	
	public Orbit orbit0; // Initial orbit of segment
	public Orbit orbitf; // Final orbit of segment
	public List<Orbit> orbits; // Intermediate segment orbits
	public Vector3d[] relativePoints; // Relative points along orbit
	public LineRenderer line; // Line drawing sail trajectory
	public double UT0; // Initial time of segment
	public double UTf; // Final time of segment
	double dT; // Step size
	GameObject obj; // Game object of line
	
	// Constructor & calculate

	public PreviewSegment(SolarSailPart sail, Orbit orbitInitial, double UT0, double UTf, SailControl control, Color color) {
	    
	    this.UT0 = UT0;
	    Debug.Log("UT0: " + UT0.ToString());
	    this.UTf = UTf;
	    Debug.Log("UTf: " + UTf.ToString());
	    dT = TimeWarp.fixedDeltaTime * control.warp;
	    Debug.Log("dT: " + dT.ToString());
	    
	    // Calculate preview orbits

	    orbits = Preview.PropagateOrbit(sail, orbitInitial, UT0, UTf, dT, control.cone, control.clock, sail.vessel.GetTotalMass());
	    orbit0 = orbits[0];
	    orbitf = orbits[orbits.Count - 1];
	    
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
	    line.SetColors(color, color);
	    line.SetVertexCount(orbits.Count);
	    // Calculate relative position vectors
	    relativePoints = new Vector3d[orbits.Count];
	    for(var i = 0; i < orbits.Count; i++) {
		double UTi = orbits[i].epoch;
		relativePoints[i] = orbits[i].getRelativePositionAtUT(UTi).xzy;
	    }
	}

	// Update segment during renders

	public void Update(Vessel vessel) {
	    if (line != null) {
		// Enable only on map
		if (MapView.MapIsEnabled) {
		    line.enabled = true;
		    // Update points
		    Vector3d rRefUT0 = vessel.orbit.referenceBody.getPositionAtUT(UT0);
		    for (int i = 0; i < orbits.Count; i++) {
			line.SetPosition(i, ScaledSpace.LocalToScaledSpace(rRefUT0 + relativePoints[i]));
		    }
		    line.SetWidth(0.01f * MapView.MapCamera.Distance, 0.01f * MapView.MapCamera.Distance);
		} else {
		    line.enabled = false;
		}
	    }
	}
    }
    
    public class Preview {
	
	// Fields
	PreviewSegment[] segments; // Trajectory segments
	SolarSailPart sail; // Sail this preview is attached to
	LineRenderer linef; // Final orbit line
	Vector3d[] linefPoints; // 3d points of final orbit
	double UTf; // final time of trajectory
	LineRenderer lineT; // Line to target
	Orbit orbitT; // Target object orbit
	public Orbit orbitf; // Final orbit

	// Target
	Vector3d rFinalRel; // Relative position of spacecraft at end of control sequence
	Vector3d rTargetFinalRel; // Relative position of target at end of control sequence
	public double targetT; // Elapsed time to closest approach
	public double targetD; // Distance to closest approach
	public double targetV; // Relative speed to target
	public double ApErr; // Error in apoapsis
	public double PeErr; // Error in periapsis
	public double TPErr; // Error in orbital period
	public double IncErr; // Error in inclination
	public double EccErr; // Error in eccentricity
	public double LANErr; // Error in longitude of ascending node
	public double AOPErr; // Error in argument of periapsis
	
	// Constructor
	
	public Preview(SolarSailPart sail) {
	    this.sail = sail;
	}

	// Calculate & draw trajectory prediction

	public void Destroy () {
	    // Destroy sail trajectory lines
	    if (segments != null) {
		foreach(var segment in segments) {
		    if (segment.line != null) {
			UnityEngine.Object.Destroy(segment.line);
		    }
		}
	    }
	    // Destroy final line
	    if (linef != null) {
		UnityEngine.Object.Destroy(linef);
	    }
	    // Destroy target line
	    if (lineT != null) {
		UnityEngine.Object.Destroy(lineT);
	    }
	}

	void CalculateTargetLine () {
	    // Selected target
	    var target = FlightGlobals.fetch.VesselTarget;
	    // If a target is selected...
	    if (target != null) {
		orbitT = target.GetOrbit(); // Target orbit
		 // Spacecraft relative position at UTf
		rFinalRel = orbitf.getRelativePositionAtUT(UTf).xzy;
		// Target relative position at UTf
		rTargetFinalRel = orbitT.getRelativePositionAtUT(UTf).xzy;
		// Distance to target at UTf
		targetD = Vector3d.Distance(rFinalRel, rTargetFinalRel);
		// Relative speed to target at UTf
		targetV = Vector3d.Distance(orbitf.getOrbitalVelocityAtUT(UTf), orbitT.getOrbitalVelocityAtUT(UTf));
		// Destroy current line if present
		if (lineT != null) {
		    UnityEngine.Object.Destroy(lineT);
		}
		// Make line to target at UTf
		var objT = new GameObject("Line to target");
		lineT = objT.AddComponent<LineRenderer>();
		lineT.useWorldSpace = false;
		objT.layer = 10; // Map
		lineT.material = MapView.fetch.orbitLinesMaterial;
		lineT.SetColors(Color.red, Color.red);
		lineT.SetVertexCount(2);

		// Target errors
		ApErr = orbitf.ApR - orbitT.ApR;
		PeErr = orbitf.PeR - orbitT.PeR;
		TPErr = orbitf.period - orbitT.period;
		IncErr = orbitf.inclination - orbitT.inclination;
		EccErr = orbitf.eccentricity - orbitT.eccentricity;
		LANErr = orbitf.LAN - orbitT.LAN;
		AOPErr = orbitf.argumentOfPeriapsis - orbitT.argumentOfPeriapsis;
	    }
	}
	
	public void Calculate () {
	    if (sail.controls.showPreview) {
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
		    segments[i] = new PreviewSegment(sail, orbitInitial, UT0, UTf, sail.controls.controls[i], sail.controls.controls[i].color);
		    // Update initial time
		    UT0 = UTf;
		    // Update initial orbit
		    orbitInitial = segments[i].orbitf;
		}

		// Final time of trajectory
		this.UTf = UT0;

		// Draw one complete final orbit

		// Destroy existing line
		if (linef != null) {
		    UnityEngine.Object.Destroy(linef);
		}
		// Create linerenderer & object
		var objf = new GameObject("Final orbit");
		linef = objf.AddComponent<LineRenderer>();
		linef.useWorldSpace = false;
		objf.layer = 10; // Map
		linef.material = MapView.fetch.orbitLinesMaterial;
		linef.SetColors(sail.controls.colorFinal, sail.controls.colorFinal);
		linef.SetVertexCount(361);
		// 3D points to use in linef
		linefPoints = new Vector3d[361];
		// Final orbit of sail trajectory
		orbitf = orbitInitial;
		// Period of final orbit
		double TPf = orbitf.period;
		// Populate points
		for(var i = 0; i <= 360; i++) {
		    double UTi = this.UTf + i * TPf / 360;
		    // Relative orbitf position
		    Vector3d rRelOrbitf = orbitf.getRelativePositionAtUT(UTi).xzy;
		    // Absolute position
		    linefPoints[i] = rRelOrbitf;
		}
		
		// Target line
		CalculateTargetLine();
	    }
	}
	

	// Update
	public void Update (Vessel vessel) {
	    if (sail.controls.showPreview) {
		if (segments != null) {
		    foreach(var segment in segments) {
			segment.Update(vessel);
		    }
		}

		// Update final orbit line from points
		if (linef != null) {
		    if (MapView.MapIsEnabled) {
			linef.enabled = true;
			// Position of reference body at end of trajectory
			Vector3d rRefUTf = vessel.orbit.referenceBody.getPositionAtUT(UTf);
			for (var i = 0; i <= 360; i++) {
			    linef.SetPosition(i, ScaledSpace.LocalToScaledSpace(rRefUTf + linefPoints[i]));
			}
			linef.SetWidth(0.01f * MapView.MapCamera.Distance, 0.01f * MapView.MapCamera.Distance);

			// Update target line
			lineT.enabled = true;
			if (FlightGlobals.fetch.VesselTarget != null) {
			    lineT.SetPosition(0, ScaledSpace.LocalToScaledSpace(rRefUTf + rFinalRel));
			    lineT.SetPosition(1, ScaledSpace.LocalToScaledSpace(rRefUTf + rTargetFinalRel));
			    lineT.SetWidth(0.01f * MapView.MapCamera.Distance, 0.01f * MapView.MapCamera.Distance);
			} else {
			    lineT.enabled = false;
			}
			
		    } else {
			linef.enabled = false;
			// lineT.enabled = false;
		    }
		}
	    }
	}

	// Propagate an orbit
	public static List<Orbit> PropagateOrbit (SolarSailPart sail, Orbit orbit0, double UT0, double UTf, double dT, float cone, float clock, double mass) {
	    Orbit orbit = orbit0.Clone();

	    int nsteps = Convert.ToInt32(Math.Ceiling((UTf - UT0) / dT));
	    double dTlast = (UTf - UT0) % dT;

	    double UT;

	    // Reseting time step to choose orbit for saving
	    double dTchoose = 0.0;

	    // List of orbits to preview
	    var orbits = new List<Orbit>();

	    // Add initial orbit
	    orbits.Add(orbit0.Clone());
	    
	    for (int i = 0; i < nsteps; i++) {
		// Last step goes to UTf
		if (i == nsteps - 1) {
		    dT = dTlast;
		    UT = UTf;
		} else {
		    UT = UT0 + i * dT;
		}

		double sunlightFactor = 1.0;
		if(!SolarSailPart.inSun(orbit, UT)) {
		    sunlightFactor = 0.0;
		}

		Quaternion sailFrame = Frames.SailFrame(orbit, cone, clock, UT);

		Vector3d normal = sailFrame * new Vector3d(0, 1, 0);

		Vector3d solarForce = SolarSailPart.CalculateSolarForce(sail, orbit, normal, UT) * sunlightFactor;

		Vector3d solarAccel = solarForce / mass / 1000.0;
		
		SolarSailPart.PerturbOrbit(orbit, solarAccel, UT, dT);

		// Increment choose time step
		dTchoose += dT;

		// Orbit period
		double TP = orbit.period;

		// Decide whether to add orbit to list of orbits to draw
		if (i == nsteps - 1) { // Always add last orbit
		    orbits.Add(orbit.Clone());
		} else if (dTchoose >= TP / 360) { // If 1/360th of current period passed, pick
		    orbits.Add(orbit.Clone());
		    dTchoose = 0.0;
		}
	    }
	    
	    // Return propagated orbit
	    return orbits;
	}
    }
}