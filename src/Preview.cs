using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PersistentThrust;

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
	double m0; // Initial mass
	double m1; // Final mass
	
	// Constructor & calculate

	public PreviewSegment(PersistentControlled engine, Orbit orbitInitial, double UT0, double UTf, Control control, Color color, double m0, double m1) {
	    Debug.Log("Preview Segment");
	    this.UT0 = UT0;
	    Debug.Log("UT0: " + UT0.ToString());
	    this.UTf = UTf;
	    Debug.Log("UTf: " + UTf.ToString());
	    dT = TimeWarp.fixedDeltaTime * control.warp;
	    Debug.Log("dT: " + dT.ToString());
	    
	    // Calculate preview orbits

	    orbits = Preview.PropagateOrbit(engine, orbitInitial, UT0, UTf, dT, control.cone, control.clock, control.throttle, m0, m1);
	    orbit0 = orbits[0];
	    orbitf = orbits[orbits.Count - 1];

	    // Update masses
	    this.m0 = m0;
	    this.m1 = m1;
	    Debug.Log("m0: " + this.m0 + ", m1: " + this.m1);
	    
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
	PersistentControlled engine; // Engine this preview is attached to
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
	
	public Preview(PersistentControlled engine) {
	    this.engine = engine;
	}

	// Calculate & draw trajectory prediction

	public void Destroy () {
	    // Destroy trajectory lines
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
	    Debug.Log("Calculate Preview");
	    if (engine.controls.showPreview) {
		// Destroy existing lines
		if (segments != null) {
		    foreach(var segment in segments) {
			if (segment.line != null) {
			    UnityEngine.Object.Destroy(segment.line);
			}
		    }
		}
		// New segments array
		segments = new PreviewSegment[engine.controls.ncontrols];
		// Beginning time
		double UT0 = engine.controls.UT0;
		Orbit orbitInitial = engine.vessel.orbit;
		// Initial mass per segment
		double m0i = engine.vessel.GetTotalMass();
		// Calculate each segment
		for (var i = 0; i < segments.Length; i++) {
		    // Final segment mass
		    double m1i = 0.0;
		    // End time
		    double UTf = UT0 + engine.controls.controls[i].duration;
		    // Calculate segment
		    segments[i] = new PreviewSegment(engine, orbitInitial, UT0, UTf, engine.controls.controls[i], engine.controls.controls[i].color, m0i, m1i);
		    // Update initial mass for next segment
		    m0i = m1i;
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
		linef.SetColors(engine.controls.colorFinal, engine.controls.colorFinal);
		linef.SetVertexCount(361);
		// 3D points to use in linef
		linefPoints = new Vector3d[361];
		// Final orbit of trajectory
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
	    if (engine.controls.showPreview) {
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
	public static List<Orbit> PropagateOrbit (PersistentControlled engine, Orbit orbit0, double UT0, double UTf, double dT, float cone, float clock, float throttle, double m0, double m1) {
	    Debug.Log("Propagate Orbit");
	    Orbit orbit = orbit0.Clone();

	    int nsteps = Convert.ToInt32(Math.Ceiling((UTf - UT0) / dT));
	    double dTlast = (UTf - UT0) % dT;

	    double UT;

	    double m0i = m0; // Current mass

	    double m1i = 0.0; // Next mass
	    
	    // Reseting time step to sample orbits for saving
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

		// Isp
		float isp = engine.atmosphereCurve.Evaluate(0);

		// Spacecraft reference frame
		Quaternion sailFrame = Frames.SailFrame(orbit, cone, clock, UT);

		// Up vector for thrust
		Vector3d up = sailFrame * new Vector3d(0, 1, 0);

		// Thrust vector
		float thrust = throttle * engine.maxThrust;

		// DeltaV vector
		Vector3d deltaVV = PersistentControlled.CalculateDeltaV(engine, dT, thrust, isp, m0i, up, m1i);

		// Update orbit
		orbit.Perturb(deltaVV, UT, dT);

		// Update mass
		m0i = m1i;
		
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

	    // Update final mass
	    m1 = m1i;
	    
	    // Return propagated orbit
	    return orbits;
	}
    }
}